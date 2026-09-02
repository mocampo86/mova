import { useCallback, useEffect, useState } from 'react';
import { Box, ButtonBase, IconButton, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';

export interface HeroSlide {
  id: string;
  src: string;
  alt: string;
  decorative?: boolean;
}

interface HeroCarouselProps {
  slides: HeroSlide[];
  interval?: number;
  overlayOpacity?: number;
  'data-testid'?: string;
}

export default function HeroCarousel({
  slides,
  interval = 0,
  overlayOpacity = 0.35,
  'data-testid': testId
}: HeroCarouselProps) {
  const { t } = useTranslation();
  const [activeIndex, setActiveIndex] = useState(0);
  const [prefersReducedMotion, setPrefersReducedMotion] = useState(false);
  const [isHovered, setIsHovered] = useState(false);
  const [focusCounter, setFocusCounter] = useState(0);
  const [failedSlides, setFailedSlides] = useState<ReadonlySet<string>>(new Set());

  const isPaused = isHovered || focusCounter > 0;
  const hasMultipleSlides = slides.length > 1;

  useEffect(() => {
    const mediaQuery = window.matchMedia?.('(prefers-reduced-motion: reduce)');
    if (!mediaQuery) {
      setPrefersReducedMotion(false);
      return;
    }
    const handler = (event: MediaQueryListEvent | MediaQueryList) => {
      setPrefersReducedMotion(event.matches);
    };
    handler(mediaQuery);
    const changeHandler = (event: MediaQueryListEvent) => handler(event);
    mediaQuery.addEventListener?.('change', changeHandler);
    return () => mediaQuery.removeEventListener?.('change', changeHandler);
  }, []);

  useEffect(() => {
    if (!hasMultipleSlides || prefersReducedMotion || isPaused || interval <= 0) return;
    const timeoutId = window.setTimeout(() => {
      setActiveIndex((prev) => (prev + 1) % slides.length);
    }, interval);
    return () => window.clearTimeout(timeoutId);
  }, [activeIndex, hasMultipleSlides, interval, isPaused, prefersReducedMotion, slides.length]);

  const goTo = useCallback((index: number) => setActiveIndex(index), []);
  const goNext = useCallback(() => setActiveIndex((prev) => (prev + 1) % slides.length), [slides.length]);
  const goPrev = useCallback(() => setActiveIndex((prev) => (prev - 1 + slides.length) % slides.length), [slides.length]);

  const handleImageError = useCallback((slideId: string) => {
    setFailedSlides((prev) => new Set([...Array.from(prev), slideId]));
  }, []);

  if (slides.length === 0) return null;

  return (
    <Box
      role="region"
      aria-roledescription="carousel"
      aria-label={t('home.hero.carousel.label')}
      data-testid={testId}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
      onFocusCapture={() => setFocusCounter((count) => count + 1)}
      onBlurCapture={() => setFocusCounter((count) => Math.max(0, count - 1))}
      sx={{ position: 'absolute', inset: 0, overflow: 'hidden' }}
    >
      {slides.map((slide, index) => {
        const isActive = index === activeIndex;
        const hasFailed = failedSlides.has(slide.id);
        return (
          <Box
            key={slide.id}
            role="group"
            aria-roledescription="slide"
            aria-hidden={!isActive}
            aria-label={t('home.hero.carousel.slideLabel', { current: index + 1, total: slides.length })}
            data-testid={`hero-slide-${slide.id}`}
            sx={{
              position: 'absolute',
              inset: 0,
              opacity: isActive ? 1 : 0,
              zIndex: isActive ? 1 : 0,
              transition: prefersReducedMotion ? 'none' : 'opacity 0.7s ease-in-out',
              backgroundImage: 'linear-gradient(135deg, #0A3D3A 0%, #14877E 100%)',
              backgroundSize: 'cover'
            }}
          >
            <img
              src={slide.src}
              alt={slide.decorative ? '' : slide.alt}
              loading={index === 0 ? 'eager' : 'lazy'}
              onError={() => handleImageError(slide.id)}
              data-testid="hero-slide-image"
              style={{
                width: '100%',
                height: '100%',
                objectFit: 'cover',
                display: hasFailed ? 'none' : 'block'
              }}
            />
          </Box>
        );
      })}

      <Box
        aria-hidden="true"
        sx={{
          position: 'absolute',
          inset: 0,
          zIndex: 2,
          bgcolor: `rgba(0, 0, 0, ${overlayOpacity})`,
          backgroundImage: 'linear-gradient(180deg, rgba(0,0,0,0.12) 0%, transparent 50%, rgba(0,0,0,0.12) 100%)'
        }}
      />

      {hasMultipleSlides && (
        <>
          <Box
            sx={{
              position: 'absolute',
              bottom: { xs: 12, sm: 16 },
              left: 0,
              right: 0,
              zIndex: 4,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              gap: 1.5
            }}
          >
            <IconButton
              size="large"
              onClick={goPrev}
              aria-label={t('home.hero.carousel.previous')}
              sx={{
                color: 'common.white',
                bgcolor: 'rgba(255,255,255,0.12)',
                border: '1px solid rgba(255,255,255,0.35)',
                '&:hover': { bgcolor: 'rgba(255,255,255,0.22)' },
                '&:focus-visible': { outline: '3px solid rgba(255,255,255,0.95)', outlineOffset: '3px' }
              }}
            >
              <Typography component="span" aria-hidden="true" sx={{ fontSize: '1.5rem', lineHeight: 1 }}>
                {'‹'}
              </Typography>
            </IconButton>

            {slides.map((slide, index) => (
              <ButtonBase
                key={slide.id}
                onClick={() => goTo(index)}
                aria-label={t('home.hero.carousel.goToSlide', { index: index + 1, total: slides.length })}
                aria-pressed={index === activeIndex}
                sx={{
                  width: 44,
                  height: 44,
                  borderRadius: '50%',
                  minWidth: 'auto',
                  p: 0,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  '&:hover .carousel-dot': { bgcolor: 'rgba(255,255,255,0.75)' },
                  '&:focus-visible': { outline: '3px solid rgba(255,255,255,0.95)', outlineOffset: '3px' }
                }}
              >
                <Box
                  className="carousel-dot"
                  sx={{
                    width: 12,
                    height: 12,
                    borderRadius: '50%',
                    bgcolor: index === activeIndex ? 'common.white' : 'rgba(255,255,255,0.35)',
                    border: '1px solid rgba(255,255,255,0.45)'
                  }}
                />
              </ButtonBase>
            ))}

            <IconButton
              size="large"
              onClick={goNext}
              aria-label={t('home.hero.carousel.next')}
              sx={{
                color: 'common.white',
                bgcolor: 'rgba(255,255,255,0.12)',
                border: '1px solid rgba(255,255,255,0.35)',
                '&:hover': { bgcolor: 'rgba(255,255,255,0.22)' },
                '&:focus-visible': { outline: '3px solid rgba(255,255,255,0.95)', outlineOffset: '3px' }
              }}
            >
              <Typography component="span" aria-hidden="true" sx={{ fontSize: '1.5rem', lineHeight: 1 }}>
                {'›'}
              </Typography>
            </IconButton>
          </Box>

          <Box
            aria-live="polite"
            aria-atomic="true"
            sx={{
              position: 'absolute',
              width: 1,
              height: 1,
              overflow: 'hidden',
              clip: 'rect(0, 0, 0, 0)',
              whiteSpace: 'nowrap',
              zIndex: -1
            }}
          >
            {t('home.hero.carousel.announcement', { current: activeIndex + 1, total: slides.length })}
          </Box>
        </>
      )}
    </Box>
  );
}
