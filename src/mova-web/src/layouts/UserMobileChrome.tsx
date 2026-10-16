import { Link as RouterLink, useLocation } from 'react-router-dom';
import { AppBar, Box, IconButton, Toolbar, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';

function isActivePath(pathname: string, to: string, exact = false) {
  return exact ? pathname === to : pathname === to || pathname.startsWith(`${to}/`);
}

function NavigationIcon({ name }: { name: 'home' | 'explore' | 'reservations' | 'profile' }) {
  const paths = {
    home: 'M4 11.5 12 5l8 6.5V20H4z M9 20v-5h6v5',
    explore: 'm11 4 6 14-3.5-2.5L10 19z M11 4 7 15.5 3.5 18z',
    reservations: 'M5 4h14v16H5z M8 8h8 M8 12h8 M8 16h5',
    profile: 'M12 12a4 4 0 1 0 0-8 4 4 0 0 0 0 8Zm-7 8c.8-3.3 3.2-5 7-5s6.2 1.7 7 5'
  };
  return <Box component="svg" viewBox="0 0 24 24" aria-hidden="true" sx={{ width: 22, height: 22, fill: 'none', stroke: 'currentColor', strokeWidth: 1.8, strokeLinecap: 'round', strokeLinejoin: 'round' }}><path d={paths[name]} /></Box>;
}

export function UserMobileHeader() {
  const { t } = useTranslation();

  return (
    <AppBar position="fixed" color="transparent" elevation={0} sx={{ display: { xs: 'flex', md: 'none' }, bgcolor: 'background.paper', color: 'text.primary', borderBottom: 1, borderColor: 'divider', zIndex: (value) => value.zIndex.drawer + 1 }}>
      <Toolbar sx={{ minHeight: 'calc(56px + env(safe-area-inset-top)) !important', pt: 'env(safe-area-inset-top)', px: 2 }}>
        <Typography component={RouterLink} to="/user" variant="h6" sx={{ color: 'primary.main', textDecoration: 'none', fontWeight: 900, letterSpacing: -0.5 }}>{t('common.appName')}</Typography>
        <Box sx={{ flexGrow: 1 }} />
        <IconButton component={RouterLink} to="/user/profile" aria-label={t('dashboard.nav.profile')} sx={{ width: 40, height: 40, color: 'primary.main', border: 1, borderColor: 'divider' }}>
          <Box component="svg" viewBox="0 0 24 24" aria-hidden="true" sx={{ width: 21, height: 21, fill: 'none', stroke: 'currentColor', strokeWidth: 1.8 }}><circle cx="12" cy="8" r="3.5" /><path d="M5 20c.9-3.4 3.2-5 7-5s6.1 1.6 7 5" /></Box>
        </IconButton>
      </Toolbar>
    </AppBar>
  );
}

export function UserBottomNav() {
  const { t } = useTranslation();
  const { pathname } = useLocation();
  const mobileNav: { label: string; to: string; icon: 'home' | 'explore' | 'reservations' | 'profile'; exact?: boolean }[] = [
    { label: t('dashboard.nav.home'), to: '/user', icon: 'home', exact: true },
    { label: t('dashboard.nav.complexes'), to: '/complexes', icon: 'explore' },
    { label: t('dashboard.nav.reservations'), to: '/user/reservations', icon: 'reservations' },
    { label: t('dashboard.nav.profile'), to: '/user/profile', icon: 'profile' }
  ];

  return (
    <Box component="nav" aria-label={t('dashboard.nav.home')} sx={{ position: 'fixed', zIndex: (value) => value.zIndex.appBar, bottom: 0, left: 0, right: 0, display: { xs: 'grid', md: 'none' }, gridTemplateColumns: 'repeat(4, minmax(0, 1fr))', width: '100%', pt: 0.5, pb: 'max(8px, env(safe-area-inset-bottom))', minHeight: 'calc(68px + env(safe-area-inset-bottom))', bgcolor: 'background.paper', borderTop: 1, borderColor: 'divider', boxShadow: '0 -4px 16px rgba(23,43,77,.08)' }}>
      {mobileNav.map((item) => {
        const active = isActivePath(pathname, item.to, item.exact);
        return <Box key={item.to} component={RouterLink} to={item.to} aria-current={active ? 'page' : undefined} sx={{ minWidth: 0, minHeight: 58, display: 'flex', alignItems: 'center', justifyContent: 'center', flexDirection: 'column', gap: 0.3, color: active ? 'primary.main' : 'text.secondary', textDecoration: 'none', fontSize: { xs: 10, sm: 11 }, fontWeight: active ? 800 : 600, overflow: 'hidden', '& > span': { display: 'block', maxWidth: '100%', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }, '&:focus-visible': { outline: '2px solid', outlineColor: 'primary.main', outlineOffset: -3 } }}><NavigationIcon name={item.icon} /><span>{item.label}</span></Box>;
      })}
    </Box>
  );
}
