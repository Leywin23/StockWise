import * as Yup from "yup";

Yup.addMethod(Yup.string, "username", function (message: string) {
  return this.matches(/^[a-zA-Z0-9_]{3,30}$/, {
    message,
    excludeEmptyString: true,
  });
});

Yup.addMethod(Yup.string, "companyNIP", function (message: string) {
  return this.matches(/^[0-9]{10}$/, {
    message,
    excludeEmptyString: true,
  });
});