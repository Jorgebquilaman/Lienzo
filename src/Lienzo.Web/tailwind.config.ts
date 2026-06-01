import type { Config } from 'tailwindcss';

const config: Config = {
  content: [
    './index.html',
    './src/**/*.{js,ts,jsx,tsx}',
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          50: '#eef2f7',
          100: '#d4dce8',
          200: '#b3c1d4',
          300: '#8da2bc',
          400: '#6b84a3',
          500: '#4e6a8a',
          600: '#3a5470',
          700: '#2a4058',
          800: '#1e2d4a',
          900: '#151f33',
          950: '#0c1424',
        },
        accent: {
          50: '#fef9e7',
          100: '#fdefc3',
          200: '#fbe39b',
          300: '#f9d46e',
          400: '#f7c44a',
          500: '#f5a623',
          600: '#d48d1c',
          700: '#b37415',
          800: '#935e0f',
          900: '#7a4d0c',
        },
        canvas: {
          DEFAULT: '#f8f7f4',
          50: '#fdfcf9',
          100: '#f8f7f4',
          200: '#f0eee8',
          300: '#e4e1d8',
          400: '#d4cfc3',
          500: '#bfb9a9',
          600: '#a59d89',
          700: '#8a826e',
          800: '#726a58',
          900: '#5e5747',
        },
      },
      fontFamily: {
        heading: ['"Playfair Display"', 'Georgia', 'serif'],
        body: ['"DM Sans"', 'system-ui', 'sans-serif'],
      },
      animation: {
        'fade-in': 'fadeIn 0.3s ease-out',
        'slide-up': 'slideUp 0.3s ease-out',
        'slide-in-right': 'slideInRight 0.3s ease-out',
        'pulse-soft': 'pulseSoft 2s ease-in-out infinite',
      },
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        slideUp: {
          '0%': { opacity: '0', transform: 'translateY(12px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        slideInRight: {
          '0%': { opacity: '0', transform: 'translateX(20px)' },
          '100%': { opacity: '1', transform: 'translateX(0)' },
        },
        pulseSoft: {
          '0%, 100%': { opacity: '1' },
          '50%': { opacity: '0.6' },
        },
      },
    },
  },
  plugins: [],
};

export default config;
