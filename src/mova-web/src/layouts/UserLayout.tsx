import { Link as RouterLink, Navigate, Outlet, useLocation } from 'react-router-dom';
import { Box, Divider, Drawer, List, ListItem, ListItemButton, ListItemText, Toolbar, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import AppHeader from './AppHeader';
import { UserBottomNav, UserMobileHeader } from './UserMobileChrome';
import { useUserDashboard } from '../features/users/useUserDashboard';

function isActivePath(pathname: string, to: string, exact = false) {
  return exact ? pathname === to : pathname === to || pathname.startsWith(`${to}/`);
}

export default function UserLayout() {
  const { t } = useTranslation();
  const { pathname } = useLocation();
  const { data, isLoading, isError } = useUserDashboard();

  if (!isLoading && isError) return <Box sx={{ display: 'grid', placeItems: 'center', minHeight: '100vh', px: 3, textAlign: 'center' }}><Box><Typography variant="h5">{t('dashboard.errorTitle')}</Typography><Typography color="text.secondary">{t('dashboard.errorMessage')}</Typography></Box></Box>;
  if (!isLoading && data?.user.phoneNumber === null) return <Navigate to="/complete-profile" replace />;

  const desktopNav = [
    { label: t('dashboard.nav.home'), to: '/user', exact: true },
    { label: t('dashboard.nav.complexes'), to: '/complexes' },
    { label: t('dashboard.nav.favorites'), to: '/user/favorites' },
    { label: t('dashboard.nav.reservations'), to: '/user/reservations' },
    { label: t('dashboard.nav.history'), to: '/user/history' },
    { label: t('dashboard.nav.recurring'), to: '/user/recurring' },
    { label: t('dashboard.nav.profile'), to: '/user/profile' }
  ];
  const greetingName = data?.user.fullName ?? data?.user.email ?? '';

  return <Box sx={{ display: 'flex', width: '100%', minWidth: 0, minHeight: '100dvh', bgcolor: 'background.default' }}>
    <Box sx={{ display: { xs: 'none', md: 'block' } }}><AppHeader greetingName={greetingName || undefined} showMenuToggle={false} /></Box>
    <UserMobileHeader />
    <Box component="nav" sx={{ width: { md: 250 }, flexShrink: { md: 0 }, display: { xs: 'none', md: 'block' } }}>
      <Drawer variant="permanent" open sx={{ '& .MuiDrawer-paper': { boxSizing: 'border-box', width: 250, borderRight: 1, borderColor: 'divider' } }}>
        <Toolbar><Typography variant="h6" fontWeight={800}>{t('common.appName')}</Typography></Toolbar><Divider />
        <List sx={{ p: 1.25 }}>{desktopNav.map((item) => {
          const active = isActivePath(pathname, item.to, item.exact);
          return <ListItem key={item.to} disablePadding><ListItemButton component={RouterLink} to={item.to} selected={active} aria-current={active ? 'page' : undefined} sx={{ borderRadius: 2, mb: 0.5, '&.Mui-selected': { bgcolor: 'primary.main', color: 'primary.contrastText', '&:hover': { bgcolor: 'primary.dark' } } }}><ListItemText primary={item.label} primaryTypographyProps={{ fontWeight: active ? 700 : 600 }} /></ListItemButton></ListItem>;
        })}</List>
      </Drawer>
    </Box>
    <Box sx={{ display: 'flex', flexDirection: 'column', flexGrow: 1, minWidth: 0, width: { xs: '100%', md: 'calc(100% - 250px)' } }}>
      <Toolbar sx={{ minHeight: { xs: 'calc(56px + env(safe-area-inset-top)) !important', md: '64px !important' } }} />
      <Box component="main" sx={{ flexGrow: 1, minWidth: 0, width: '100%', pb: { xs: 'calc(76px + env(safe-area-inset-bottom))', md: 3 } }}><Outlet /></Box>
    </Box>
    <UserBottomNav />
  </Box>;
}
