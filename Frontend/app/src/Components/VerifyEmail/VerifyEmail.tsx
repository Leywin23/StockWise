import React, { useState } from 'react'
import { verifyEmailFromApi } from '../../api';

const VerifyEmail : React.FC = () => {
    const [result, setResult] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [form, setForm] = useState({
        email: "",
        code: ""
    });
    

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>)=>{
        const {name, value} = e.target;
        setForm((prev)=>({...prev, [name]: value}));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSubmitting(true);
        setError(null);
        setResult(null);
    
        try{
            const res = await verifyEmailFromApi(form.email, form.code)
            setResult(res);
        } catch (err: any) {
  let message = "Unknown error";

  if (err?.response?.data?.message) {
    // np. własne pole "message" z API
    message = err.response.data.message;
  } else if (err?.response?.data?.detail) {
    // ASP.NET ProblemDetails ma "detail"
    message = err.response.data.detail;
  } else if (typeof err?.response?.data === "string") {
    // jeśli backend zwrócił zwykły tekst
    message = err.response.data;
  } else if (err?.message) {
    message = err.message;
  } else if (err?.response?.data) {
    // fallback – stringify całego obiektu
    message = JSON.stringify(err.response.data);
  }

  setError(message);
  console.error("Error verifying email:", err);
} finally {
  setIsSubmitting(false);
}
    };
    
  return (
    <div>VerifyEmail
        <form onSubmit={handleSubmit}>
            <input type="text" name='email' value={form.email} onChange={handleChange} required/>
            <input type='text' name = 'code' value={form.code} onChange={handleChange} required/>
            <button type='submit' disabled={isSubmitting}>
                {isSubmitting ? "Creating..." : "Create"}
            </button>
        </form>
        {error && (
        <p>Error: {error}</p>
        )}

        {result && (
        <div>
            <h3>Veryfication passed successfully:</h3>
            <pre>{JSON.stringify(result, null, 2)}</pre>
        </div>
        )}
    </div>
  )

}
export default VerifyEmail;