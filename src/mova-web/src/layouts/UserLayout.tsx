import { useState } from 'react';
import { Link as RouterLink, Navigate, Outlet, useLocation } from 'react-router-dom';
import {
  AppBar,
  Box,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  Skeleton,
  Toolbar,
  Typography,
  useMediaQuery,
  useTheme
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import LanguageSelector from '../components/LanguageSelector';
import { useAuth } from '../features/auth/useAuth';
import { useUserDashboard } from '../features/users/useUserDashboard';

function isActivePath(pathname: string, to: string, exact = false): boolean {
  const normalized = to.endsWith('/') ? to.slice(0, -1) : to;
  if (pathname === normalized) return true;
  if (exact) return false;
  return pathname.startsWith(`${normalized}/`);
}

export default function UserLayout() {
  const { t } = useTranslation();
  const { pathname } = useLocation();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const [mobileOpen, setMobileOpen] = useState(false);
  const { logout } = useAuth();
  const { data, isLoading, isError } = useUserDashboard();

  const handleDrawerToggle = () => setMobileOpen((open) => !open);

  if (!isLoading && isError) {
    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', height: '100vh', gap: 2, px: 2 }}>
        <Typography variant="h5">{t('dashboard.errorTitle')}</Typography>
        <Typography color="text.secondary" textAlign="center">
          {t('dashboard.errorMessage')}
        </Typography>
      </Box>
    );
  }

  if (!isLoading && data?.user.phoneNumber === null) {
    return <Navigate to="/complete-profile" replace />;
  }

  const navItems = [
    { label: t('dashboard.nav.home'), to: '/user', exact: true },
    { label: t('dashboard.nav.complexes'), to: '/complexes' },
    { label: t('dashboard.nav.reservations'), to: '/user/reservations' },
    { label: t('dashboard.nav.history'), to: '/user/history' },
    { label: t('dashboard.nav.recurring'), to: '/user/recurring' },
    { label: t('dashboard.nav.profile'), to: '/user/profile' }
  ];

  const drawerContent = (
    <>
      <Toolbar>
        <Typography variant="h6" noWrap component="div">
          {t('common.appName')}
        </Typography>
      </Toolbar>
      <Divider />
      <List>
        {navItems.map((item) => {
          const active = isActivePath(pathname, item.to, item.exact === true);
          return (
            <ListItem key={item.label} disablePadding>
              <ListItemButton
                component={RouterLink}
                to={item.to}
                selected={active}
                onClick={isMobile ? handleDrawerToggle : undefined}
                aria-current={active ? 'page' : undefined}
              >
                <ListItemText primary={item.label} />
              </ListItemButton>
            </ListItem>
          );
        })}
      </List>
    </>
  );

  const greetingName = data?.user.fullName ?? data?.user.email ?? '';

  return (
    <Box sx={{ display: 'flex' }}>
      <AppBar
        position="fixed"
        sx={{ zIndex: (theme) => theme.zIndex.drawer + 1 }}
      >
        <Toolbar>
          {isMobile && (
            <IconButton
              color="inherit"
              aria-label={t('nav.openMenu')}
              edge="start"
              onClick={handleDrawerToggle}
              sx={{ mr: 2 }}
            >
              <Typography component="span" sx={{ fontSize: '1.5rem', lineHeight: 1 }}>
                ☰
              </Typography>
            </IconButton>
          )}
          <Typography variant="h6" noWrap component="div" sx={{ flexGrow: 1 }}>
            {isLoading || !greetingName ? (
              <Skeleton variant="text" width={180} />
            ) : (
              t('dashboard.welcome', { name: greetingName })
            )}
          </Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <LanguageSelector />
            {!isMobile && (
              <IconButton color="inherit" onClick={logout} aria-label={t('dashboard.logout')}>
                <Typography component="span" sx={{ fontSize: '0.875rem' }}>
                  {t('dashboard.logout')}
                </Typography>
              </IconButton>
            )}
          </Box>
        </Toolbar>
      </AppBar>
      <Box
        component="nav"
        sx={{
          width: { md: 250 },
          flexShrink: { md: 0 },
          display: { xs: 'none', md: 'block' }
        }}
      >
        <Drawer
          variant="permanent"
          open
          sx={{ '& .MuiDrawer-paper': { boxSizing: 'border-box', width: 250 } }}
        >
          {drawerContent}
        </Drawer>
      </Box>
      {isMobile && (
        <Drawer
          variant="temporary"
          open={mobileOpen}
          onClose={handleDrawerToggle}
          ModalProps={{ keepMounted: true }}
          sx={{ '& .MuiDrawer-paper': { boxSizing: 'border-box', width: 250 } }}
        >
          {drawerContent}
        </Drawer>
      )}
      <Box component="main" sx={{ flexGrow: 1, width: { md: 'calc(100% - 250px)' } }}>
        <Toolbar />
        <Outlet />
      </Box>
    </Box>
  );
}
