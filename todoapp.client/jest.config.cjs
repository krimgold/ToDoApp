module.exports = {
  preset: 'ts-jest',
  testEnvironment: 'jest-environment-jsdom',
  setupFilesAfterEnv: ['<rootDir>/src/setupTests.ts'],
  // use ts-jest transformer and pass ts-jest config here (recommended)
  transform: {
    '^.+\\.(ts|tsx)$': ['ts-jest', { tsconfig: 'tsconfig.jest.json', diagnostics: false }]
  },
  moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx', 'json', 'node']
};
// map css imports to identity-obj-proxy for tests
module.exports.moduleNameMapper = {
  '\\.(css|less|scss|sass)$': 'identity-obj-proxy'
};
