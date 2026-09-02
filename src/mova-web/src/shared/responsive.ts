import type { SxProps, Theme } from '@mui/material/styles';

export const responsiveCtaSx: SxProps<Theme> = {
  whiteSpace: 'normal',
  lineHeight: 1.25,
  minHeight: 48,
  height: 'auto',
  width: { xs: '100%', sm: 'auto' }
};
