import { useEffect } from 'react';

interface SeoProps {
  title: string;
  description: string;
  ogTitle?: string;
  ogDescription?: string;
  ogType?: string;
  ogUrl?: string;
}

export default function Seo({
  title,
  description,
  ogTitle,
  ogDescription,
  ogType = 'website',
  ogUrl
}: SeoProps) {
  useEffect(() => {
    document.title = title;

    updateOrCreateMeta('name', 'description', description);
    updateOrCreateMeta('property', 'og:title', ogTitle ?? title);
    updateOrCreateMeta('property', 'og:description', ogDescription ?? description);
    updateOrCreateMeta('property', 'og:type', ogType);
    updateOrCreateMeta('property', 'og:url', ogUrl ?? (typeof window !== 'undefined' ? window.location.href : ''));
  }, [title, description, ogTitle, ogDescription, ogType, ogUrl]);

  return null;
}

function updateOrCreateMeta(attr: 'name' | 'property', value: string, content: string): void {
  const selector = `meta[${attr}="${value}"]`;
  let meta = document.querySelector<HTMLMetaElement>(selector);

  if (!meta) {
    meta = document.createElement('meta');
    meta.setAttribute(attr, value);
    document.head.appendChild(meta);
  }

  meta.setAttribute('content', content);
}
