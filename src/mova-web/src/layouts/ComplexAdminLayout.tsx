import { useState } from 'react';
import { Link as RouterLink, Outlet, useLocation, useParams } from 'react-router-dom';
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
import { useComplexDashboard } from '../features/complexes/complexApi';

function isActivePath(pathname: string, to: string, exact = false): boolean {
  const normalized = to.endsWith('/') ? to.slice(0, -1) : to;
  if (pathname === normalized) return true;
  if (exact) return false;
  return pathname.startsWith(`${normalized}/`);
}

export default function ComplexAdminLayout() {
  const { t } = useTranslation();
  const { complexId = '' } = useParams<{ complexId: string }>();
  const { pathname } = useLocation();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const [mobileOpen, setMobileOpen] = useState(false);
  const { data, isLoading } = useComplexDashboard(complexId);

  const handleDrawerToggle = () => setMobileOpen((open) => !open);

  const navItems = [
    { label: t('nav.dashboard'), to: `/admin/complex/${complexId}`, exact: true },
    { label: t('nav.profile'), to: `/admin/complex/${complexId}/profile` },
    { label: t('nav.businessHours'), to: `/admin/complex/${complexId}/business-hours` },
    { label: t('nav.courts'), to: `/admin/complex/${complexId}/courts` },
    { label: t('nav.reservations'), to: `/admin/complex/${complexId}/reservations` },
    { label: t('nav.users'), to: `/admin/complex/${complexId}/users` }
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

  const headerTitle = isLoading ? (
    <Skeleton variant="text" width={160} />
  ) : (
    data?.complex.name ?? t('nav.complexAdmin')
  );

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
            {headerTitle}
          </Typography>
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
