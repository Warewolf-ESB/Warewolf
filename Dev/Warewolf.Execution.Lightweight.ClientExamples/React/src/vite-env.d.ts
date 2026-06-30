/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_TENANT_ID: string;
  readonly VITE_SPA_CLIENT_ID: string;
  readonly VITE_RESOURCE_APP_ID: string;
  readonly VITE_FUNCTION_APP_URL: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
