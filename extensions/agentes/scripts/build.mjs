import { build } from 'esbuild';
import { readFileSync } from 'fs';

const watch = process.argv.includes('--watch');

const pkg = JSON.parse(readFileSync('./package.json','utf8'));

build({
  entryPoints: ['src/extension.ts'],
  outfile: 'dist/extension.js',
  bundle: true,
  platform: 'node',
  target: 'node18',
  sourcemap: true,
  external: ['vscode'],
  watch: watch && {
    onRebuild(error) {
      if (error) console.error('Rebuild failed', error);
      else console.log('Rebuilt');
    }
  }
}).then(()=> console.log('Build ok', pkg.version)).catch(e=>{ console.error(e); process.exit(1); });
