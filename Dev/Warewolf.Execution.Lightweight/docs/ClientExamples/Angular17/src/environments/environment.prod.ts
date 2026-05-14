/**
 * environment.prod.ts — Production environment overrides.
 * Replace placeholder values before deploying.
 */
export const environment = {
  production: true,

  entra: {
    tenantId:      'ca0cc53b-9af4-4067-bcdf-be9c648450d1',
    spaClientId:   '1a328b9c-2f07-4ac4-b384-ad8ef23fdbb7',
    resourceAppId: '05b557d6-e4b3-45bb-ad1c-23182e3060a2',
  },

  functionAppUrl: 'https://wwexecution.azurewebsites.net',

  get authority(): string {
    return `https://login.microsoftonline.com/${this.entra.tenantId}`;
  },

  get userScope(): string {
    return `api://${this.entra.resourceAppId}/user_impersonation`;
  },

  get redirectUri(): string {
    return 'https://<your-app>.azurestaticapps.net';
  },
};
