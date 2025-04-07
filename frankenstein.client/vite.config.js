import { fileURLToPath, URL } from 'node:url';

import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';

const target = 'http://95.161.164.28:51161';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url))
        }
    },
    server: {
        https:false,
        host: '0.0.0.0',
        port: 51160,
        proxy: {
            '^/matrix': {
                target,
                secure: false
            }
        // },
        // port: parseInt(env.DEV_SERVER_PORT || '51160'),
        // http: {
        //     key: fs.readFileSync(keyFilePath),
        //     cert: fs.readFileSync(certFilePath),
        }
    }
})
