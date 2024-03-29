import esbuild from 'esbuild'
import { altvEsbuild } from 'altv-esbuild'

const dev = process.argv[2] === '-dev'

console.log('dev:', dev)

esbuild.build({
  watch: dev,
  bundle: true,
  format: 'esm',
  target: "esnext",
  logLevel: "info",
  entryPoints: ['src/index.ts'],
  outfile: 'dist/client.js',

  plugins: [
    altvEsbuild({
      mode: 'client',
      dev,
      altvEnums: true,
    })
  ]
})
