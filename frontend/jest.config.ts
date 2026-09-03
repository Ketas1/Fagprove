import type { Config } from 'jest';
import nextJest from 'next/jest.js';

// next/jest wires up SWC, the tsconfig path aliases, CSS module stubs and
// next.config, so none of that has to be configured by hand.
const createJestConfig = nextJest({ dir: './' });

const config: Config = {
  coverageProvider: 'v8',
  testEnvironment: 'jsdom',
  setupFilesAfterEnv: ['<rootDir>/jest.setup.ts'],
  testPathIgnorePatterns: ['<rootDir>/.next/', '<rootDir>/node_modules/'],
};

export default createJestConfig(config);
