"""Preserve complete subprocess diagnostics and make CI failures inspectable."""
import subprocess
import sys

if len(sys.argv) < 2:
    raise SystemExit('Pass an executable and its arguments')
result = subprocess.run(sys.argv[1:], stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
print(result.stdout, end='', flush=True)
if result.returncode:
    message = result.stdout[-6000:].replace('%', '%25').replace('\r', '%0D').replace('\n', '%0A')
    print('::error title=Standalone verification failed::' + message, flush=True)
    raise SystemExit(result.returncode if result.returncode > 0 else 1)
