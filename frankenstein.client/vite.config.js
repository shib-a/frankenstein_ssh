import { fileURLToPath, URL } from 'node:url';

import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import { env } from 'process';

const target = `http://localhost:${env.ASPNETCORE_HTTP_PORT}`;

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
