import React, { useState } from 'react';
import { CompanyWithAccountDto, CreateCompanyWithAccountDto, createCompanyWithAccountFromApi, CreateProductDto, postProductToApi } from '../../../api';
import { json } from 'stream/consumers';

export const CreateCompanyWithAccount: React.FC = () => {
  const [form, setForm] = useState<CreateCompanyWithAccountDto>({
    userName: "",
    email: "",
    password: "",
    companyName: "",
    nip: "",
    companyEmail: "",
    address: "",
    phone: "",
  });

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [result, setResult] = useState<CompanyWithAccountDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    const {name, value} = e.target;
    setForm((prev)=> ({ ...prev, [name]: value}));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setError(null);
    setResult(null);
  

 try {
      const res = await createCompanyWithAccountFromApi(form);
      setResult(res);
    } catch (err: any) {
      const message =
        err?.response?.data?.message ||
        err?.response?.data ||
        err?.message ||
        "Unknown error";
      setError(message);
      console.error("Error creating company with account:", err);
    } finally {
      setIsSubmitting(false);
    }
  };
  
  return (<div>
    <h2>Create Company with Account</h2>
    <form onSubmit={handleSubmit}>
        <div>
            <label>User Name
                <input
                    type="text"
                    name = "userName"
                    value={form.userName}
                    onChange={handleChange}
                    required
                />
            </label>
        </div>
        <div>
            <label>
                Email
                <input
                    type="text"
                    name='email'
                    value={form.email}
                    onChange={handleChange}
                    required
                />
            </label>
        </div>

        <div>
          <label>
            Password
            <input
              type="password"
              name="password"
              value={form.password}
              onChange={handleChange}
              required
            />
          </label>
        </div>

        <div>
          <label>
            Company name
            <input
              type="text"
              name="companyName"
              value={form.companyName}
              onChange={handleChange}
              required
            />
          </label>
        </div>

        <div>
          <label>
            NIP
            <input
              type="text"
              name="nip"
              value={form.nip}
              onChange={handleChange}
              required
            />
          </label>
        </div>

        <div>
          <label>
            Company email
            <input
              type="email"
              name="companyEmail"
              value={form.companyEmail}
              onChange={handleChange}
              required
            />
          </label>
        </div>

        <div>
          <label>
            Address
            <input
              type="text"
              name="address"
              value={form.address}
              onChange={handleChange}
              required
            />
          </label>
        </div>

        <div>
          <label>
            Phone
            <input
              type="tel"
              name="phone"
              value={form.phone}
              onChange={handleChange}
              required
            />
          </label>
        </div>

        <button type ="submit" disabled={isSubmitting}>
            {isSubmitting ? "Creating..." : "Create"}
        </button>
    </form>

    {error && (
        <p>Error: {error}</p>
    )}

    {result && (
        <div>
            <h3>Created successfully:</h3>
            <pre>{JSON.stringify(result, null, 2)}</pre>
        </div>
    )}
  </div>
  );
}
export default CreateCompanyWithAccount;