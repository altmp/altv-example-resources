import esbuild from "esbuild"
import { altvEsbuild } from 'altv-esbuild'

const dev = process.argv[2] === '-dev'
console.log('dev:', dev)

esbuild.build({
  watch: dev,
  bundle: true,
  format: 'esm',
  target: "esnext",
  logLevel: "info",
  platform: 'node',
  entryPoints: ['src-server/index.ts'],
  outfile: 'dist/server.js',

  plugins: [
    altvEsbuild({
      mode: 'server',
      dev,
    }),
  ]
})
