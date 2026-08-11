const gatewayUrl =
  process.env.services__gateway__https__0 ??
  process.env.services__gateway__http__0;

if (!gatewayUrl) {
  throw new Error('Aspire did not provide the Gateway endpoint. Start the web app through AppHost.');
}

module.exports = {
  '/api': {
    target: gatewayUrl,
    secure: false,
    changeOrigin: true,
    logLevel: 'info'
  }
};
