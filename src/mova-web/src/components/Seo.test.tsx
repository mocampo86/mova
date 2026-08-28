import { cleanup, render } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import Seo from './Seo';

function resetHead() {
  document.title = '';
  document.head.querySelectorAll('meta[name="description"], meta[property^="og:"]').forEach((el) => el.remove());
}

beforeEach(() => {
  resetHead();
});

afterEach(() => {
  cleanup();
});

describe('Seo', () => {
  it('sets the document title and meta description', () => {
    render(<Seo title="Mova | Home" description="Home description" />);

    expect(document.title).toBe('Mova | Home');

    const descriptionMeta = document.querySelector('meta[name="description"]');
    expect(descriptionMeta).not.toBeNull();
    expect(descriptionMeta?.getAttribute('content')).toBe('Home description');
  });

  it('sets Open Graph meta tags', () => {
    render(<Seo title="Mova | Home" description="Home description" ogUrl="https://example.com/" />);

    expect(document.querySelector('meta[property="og:title"]')?.getAttribute('content')).toBe('Mova | Home');
    expect(document.querySelector('meta[property="og:description"]')?.getAttribute('content')).toBe('Home description');
    expect(document.querySelector('meta[property="og:type"]')?.getAttribute('content')).toBe('website');
    expect(document.querySelector('meta[property="og:url"]')?.getAttribute('content')).toBe('https://example.com/');
  });

  it('falls back to window.location.href when ogUrl is not provided', () => {
    render(<Seo title="Mova | Home" description="Home description" />);

    expect(document.querySelector('meta[property="og:url"]')?.getAttribute('content')).toBe(window.location.href);
  });

  it('updates existing meta tags instead of duplicating them', () => {
    const { unmount } = render(<Seo title="First" description="First description" />);
    unmount();

    render(<Seo title="Second" description="Second description" />);

    expect(document.querySelectorAll('meta[name="description"]').length).toBe(1);
    expect(document.querySelectorAll('meta[property="og:title"]').length).toBe(1);
    expect(document.title).toBe('Second');
    expect(document.querySelector('meta[name="description"]')?.getAttribute('content')).toBe('Second description');
  });

  it('uses custom ogTitle and ogDescription when provided', () => {
    render(
      <Seo
        title="Mova | Home"
        description="Home description"
        ogTitle="Open Graph title"
        ogDescription="Open Graph description"
      />
    );

    expect(document.querySelector('meta[property="og:title"]')?.getAttribute('content')).toBe('Open Graph title');
    expect(document.querySelector('meta[property="og:description"]')?.getAttribute('content')).toBe('Open Graph description');
  });
});
