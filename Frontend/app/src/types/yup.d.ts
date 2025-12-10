import "yup";

declare module "yup" {
  interface StringSchema<
    TType = string | undefined,
    TContext = any,
    TDefault = undefined,
    TFlags extends import("yup").Flags = ""
  > {
    username(message?: string): this;
    companyNIP(message?: string): this;
  }
}
