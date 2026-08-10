import { describe, expect, it } from 'vitest';
import { getLanguageFlag, LANGUAGE_FLAGS } from './index';

describe('flag assets', () => {
  it('provides a flag for each supported language', () => {
    expect(LANGUAGE_FLAGS.en).toBe('🇬🇧');
    expect(LANGUAGE_FLAGS.es).toBe('🇪🇸');
    expect(LANGUAGE_FLAGS.pt).toBe('🇵🇹');
  });

  it('returns an empty string for unsupported languages', () => {
    expect(getLanguageFlag('fr')).toBe('');
  });
});
