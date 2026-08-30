import { act, fireEvent, screen } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import HeroCarousel from './HeroCarousel';
import { renderWithAuth } from '../test-utils';

const defaultSlides = [
  { id: 'padel', src: '/images/hero/padel.svg', alt: '', decorative: true },
  { id: 'basketball', src: '/images/hero/basketball.svg', alt: '', decorative: true },
  { id: 'tennis', src: '/images/hero/tennis.svg', alt: '', decorative: true }
];

function mockMatchMedia(matches: boolean) {
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    value: vi.fn().mockImplementation((query: string) => ({
      matches: matches && query === '(prefers-reduced-motion: reduce)',
      media: query,
      onchange: null,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn()
    }))
  });
}

function getSlide(id: string) {
  return screen.getByTestId(`hero-slide-${id}`);
}

function isSlideActive(slide: HTMLElement) {
  return slide.getAttribute('aria-hidden') !== 'true';
}

describe('HeroCarousel', () => {
  beforeEach(() => {
    mockMatchMedia(false);
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.restoreAllMocks();
  });

  it('renders a labelled carousel region with the first slide active', () => {
    renderWithAuth(<HeroCarousel slides={defaultSlides} />);
    expect(screen.getByRole('region', { name: 'Sports facility images' })).toBeTruthy();
    expect(isSlideActive(getSlide('padel'))).toBe(true);
    expect(screen.getByText('Showing slide 1 of 3')).toBeTruthy();
  });

  it('advances to the next slide when the next button is activated', () => {
    renderWithAuth(<HeroCarousel slides={defaultSlides} />);
    fireEvent.click(screen.getByRole('button', { name: 'Next slide' }));
    expect(isSlideActive(getSlide('basketball'))).toBe(true);
    expect(screen.getByText('Showing slide 2 of 3')).toBeTruthy();
  });

  it('wraps to the last slide when previous is activated from the first slide', () => {
    renderWithAuth(<HeroCarousel slides={defaultSlides} />);
    fireEvent.click(screen.getByRole('button', { name: 'Previous slide' }));
    expect(isSlideActive(getSlide('tennis'))).toBe(true);
    expect(screen.getByText('Showing slide 3 of 3')).toBeTruthy();
  });

  it('jumps to a specific slide when an indicator is activated', () => {
    renderWithAuth(<HeroCarousel slides={defaultSlides} />);
    fireEvent.click(screen.getByRole('button', { name: 'Go to slide 3 of 3' }));
    expect(isSlideActive(getSlide('tennis'))).toBe(true);
  });

  it('does not render controls and does not auto-advance with a single slide', () => {
    vi.useFakeTimers();
    renderWithAuth(<HeroCarousel slides={[defaultSlides[0]]} interval={1000} />);
    expect(screen.queryByRole('button', { name: 'Next slide' })).toBeNull();
    expect(screen.queryByRole('button', { name: 'Previous slide' })).toBeNull();
    act(() => {
      vi.advanceTimersByTime(5000);
    });
    expect(isSlideActive(getSlide('padel'))).toBe(true);
  });

  it('does not auto-advance when reduced motion is preferred', () => {
    mockMatchMedia(true);
    vi.useFakeTimers();
    renderWithAuth(<HeroCarousel slides={defaultSlides} interval={1000} />);
    act(() => {
      vi.advanceTimersByTime(5000);
    });
    expect(isSlideActive(getSlide('padel'))).toBe(true);
  });

  it('auto-advances after the configured interval', () => {
    vi.useFakeTimers();
    renderWithAuth(<HeroCarousel slides={defaultSlides} interval={1000} />);
    act(() => {
      vi.advanceTimersByTime(1000);
    });
    expect(isSlideActive(getSlide('basketball'))).toBe(true);
    act(() => {
      vi.advanceTimersByTime(1000);
    });
    expect(isSlideActive(getSlide('tennis'))).toBe(true);
  });

  it('keeps controls usable when an image fails to load', () => {
    renderWithAuth(<HeroCarousel slides={defaultSlides} />);
    const image = screen.getAllByTestId('hero-slide-image')[0];
    fireEvent.error(image);
    expect(image.style.display).toBe('none');
    fireEvent.click(screen.getByRole('button', { name: 'Next slide' }));
    expect(isSlideActive(getSlide('basketball'))).toBe(true);
  });

  it('pauses auto-advance on hover', () => {
    vi.useFakeTimers();
    renderWithAuth(<HeroCarousel slides={defaultSlides} interval={1000} />);
    const carousel = screen.getByRole('region', { name: 'Sports facility images' });
    fireEvent.mouseEnter(carousel);
    act(() => {
      vi.advanceTimersByTime(2000);
    });
    expect(isSlideActive(getSlide('padel'))).toBe(true);
    fireEvent.mouseLeave(carousel);
    act(() => {
      vi.advanceTimersByTime(1000);
    });
    expect(isSlideActive(getSlide('basketball'))).toBe(true);
  });

  it('exposes accessible names and pressed state on indicators', () => {
    renderWithAuth(<HeroCarousel slides={defaultSlides} />);
    const indicator = screen.getByRole('button', { name: 'Go to slide 2 of 3' });
    expect(indicator.getAttribute('aria-pressed')).toBe('false');
    fireEvent.click(indicator);
    expect(indicator.getAttribute('aria-pressed')).toBe('true');
  });
});
