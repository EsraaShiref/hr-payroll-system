const PROXY_CONFIG = {
  '/api/*': {
    target: 'https://localhost:5001',
    secure: false,
    logLevel: 'debug',
  },
};

module.exports = PROXY_CONFIG;
