import type { ReactNode } from 'react';
import { Box } from '@mui/material';
import type { SxProps, Theme } from '@mui/material/styles';

export type UserContentWidth = 'wide' | 'form' | 'compact';

const maxWidths: Record<UserContentWidth, number> = {
  wide: 1200,
  form: 900,
  compact: 720
};

interface UserContentContainerProps {
  children: ReactNode;
  width?: UserContentWidth;
  sx?: SxProps<Theme>;
}

export default function UserContentContainer({
  children,
  width = 'wide',
  sx
}: UserContentContainerProps) {
  return (
    <Box
      sx={[
        {
          width: '100%',
          maxWidth: maxWidths[width],
          minWidth: 0,
          mx: 0,
          px: { xs: 2, md: 4 },
          py: { xs: 2.5, md: 4 }
        },
        ...(Array.isArray(sx) ? sx : [sx])
      ]}
    >
      {children}
    </Box>
  );
}
