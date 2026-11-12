const gatewayUrl =
  process.env.services__gateway__https__0 ??
  process.env.services__gateway__http__0;
const identityUrl =
  process.env['services__identity-api__http__0'] ??
  process.env['services__identity-api__https__0'];

if (!gatewayUrl) {
  throw new Error('Aspire did not provide the Gateway endpoint. Start the web app through AppHost.');
}
if (!identityUrl) {
  throw new Error('Aspire did not provide the IdentityAccess endpoint. Start the web app through AppHost.');
}

module.exports = {
  '/identity-demo-api': {
    target: identityUrl,
    secure: false,
    changeOrigin: true,
    pathRewrite: { '^/identity-demo-api': '' },
    logLevel: 'info'
  },
  '/api': {
    target: gatewayUrl,
    secure: false,
    changeOrigin: true,
    logLevel: 'info'
  }
};
