import { describe, expect, it } from 'vitest';
import { getLanguageFlag, LANGUAGE_FLAGS } from './index';

describe('flag assets', () => {
  it('provides a flag asset for each supported language', () => {
    expect(LANGUAGE_FLAGS.en).toBeTruthy();
    expect(LANGUAGE_FLAGS.es).toBeTruthy();
    expect(LANGUAGE_FLAGS.pt).toBeTruthy();
  });

  it('returns an empty string for unsupported languages', () => {
    expect(getLanguageFlag('fr')).toBe('');
  });
});
