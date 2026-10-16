import { useState } from 'react';
import { Link as RouterLink, Outlet, useLocation, useParams } from 'react-router-dom';
import {
  AppBar,
  Box,
  Chip,
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
import { useAdminComplex } from '../features/complexes/complexApi';

function isActivePath(pathname: string, to: string, exact = false): boolean {
  const normalized = to.endsWith('/') ? to.slice(0, -1) : to;
  if (pathname === normalized) return true;
  if (exact) return false;
  return pathname.startsWith(`${normalized}/`);
}

function NavMark({ label }: { label: string }) {
  return (
    <Box
      className="nav-mark"
      aria-hidden="true"
      sx={{
        width: 26,
        height: 26,
        mr: 1.5,
        borderRadius: 1.5,
        display: 'grid',
        placeItems: 'center',
        bgcolor: 'rgba(37, 99, 235, 0.1)',
        color: 'primary.main',
        fontSize: 10,
        fontWeight: 800
      }}
    >
      {label.slice(0, 2).toUpperCase()}
    </Box>
  );
}

export default function ComplexAdminLayout() {
  const { t } = useTranslation();
  const { complexId = '' } = useParams<{ complexId: string }>();
  const { pathname } = useLocation();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const [mobileOpen, setMobileOpen] = useState(false);
  const { data, isLoading } = useAdminComplex(complexId);

  const handleDrawerToggle = () => setMobileOpen((open) => !open);

  const navItems = [
    { label: t('nav.dashboard'), to: `/admin/complex/${complexId}`, exact: true },
    { label: t('nav.profile'), to: `/admin/complex/${complexId}/profile` },
    { label: t('nav.businessHours'), to: `/admin/complex/${complexId}/business-hours` },
    { label: t('nav.courts'), to: `/admin/complex/${complexId}/courts` },
    { label: t('nav.reservations'), to: `/admin/complex/${complexId}/reservations` },
    { label: t('nav.recurring'), to: `/admin/complex/${complexId}/recurring` },
    { label: t('nav.users'), to: `/admin/complex/${complexId}/users` },
    { label: t('nav.configuration'), to: `/admin/complex/${complexId}/configuration` }
  ];

  const drawerContent = (
    <>
      <Toolbar sx={{ px: 2.5 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.25 }}>
          <Box sx={{ width: 30, height: 30, borderRadius: 1.5, bgcolor: 'primary.main', color: 'white', display: 'grid', placeItems: 'center', fontWeight: 900 }}>
            M
          </Box>
          <Typography variant="h6" noWrap component="div" sx={{ fontWeight: 800, letterSpacing: '-0.03em' }}>
            {t('common.appName')}
          </Typography>
        </Box>
      </Toolbar>
      <Divider />
      <List sx={{ p: 1.25 }}>
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
                sx={{
                  minHeight: 46,
                  borderRadius: 2,
                  mb: 0.5,
                  '&.Mui-selected': {
                    bgcolor: 'primary.main',
                    color: 'primary.contrastText',
                    '& .nav-mark': { bgcolor: 'rgba(255,255,255,0.18)', color: 'inherit' },
                    '&:hover': { bgcolor: 'primary.dark' }
                  }
                }}
              >
                <NavMark label={item.label} />
                <ListItemText primary={item.label} primaryTypographyProps={{ fontWeight: active ? 700 : 600, variant: 'body2' }} />
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
    data?.name ?? t('nav.complexAdmin')
  );

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh', bgcolor: 'background.default' }}>
      <AppBar
        position="fixed"
        color="inherit"
        elevation={0}
        sx={{
          zIndex: (theme) => theme.zIndex.drawer + 1,
          bgcolor: 'background.paper',
          borderBottom: 1,
          borderColor: 'divider'
        }}
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
          <Box sx={{ flexGrow: 1, minWidth: 0 }}>
            <Typography variant="subtitle2" color="text.secondary" noWrap>
              {t('nav.complexAdmin')}
            </Typography>
            <Typography variant="h6" noWrap component="div">
              {headerTitle}
            </Typography>
          </Box>
          {!isLoading && data?.status && (
            <Chip
              label={data.status}
              color={data.status === 'Active' ? 'success' : 'default'}
              size="small"
              sx={{ mr: 2, fontWeight: 700 }}
            />
          )}
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <LanguageSelector />
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
          sx={{
            '& .MuiDrawer-paper': {
              boxSizing: 'border-box',
              width: 250,
              borderRight: 1,
              borderColor: 'divider',
              bgcolor: 'background.paper'
            }
          }}
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
      <Box sx={{ display: 'flex', flexDirection: 'column', flexGrow: 1, width: { md: 'calc(100% - 250px)' } }}>
        <Toolbar />
        <Box component="main" sx={{ flexGrow: 1, py: { xs: 1, sm: 2 } }}>
          <Outlet />
        </Box>
      </Box>
    </Box>
  );
}
