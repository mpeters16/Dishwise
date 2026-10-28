#!/bin/sh
set -e

# Default BACKEND_URL if not set
BACKEND_URL=${BACKEND_URL:-}

echo "==> BACKEND_URL=${BACKEND_URL}"

# Inject BACKEND_URL into index.html as a global variable
if [ -n "$BACKEND_URL" ]; then
  sed -i "s|</head>|<script>window.__BACKEND_URL__=\"${BACKEND_URL}\";</script></head>|" /usr/share/nginx/html/index.html
  echo "==> Injected __BACKEND_URL__ into index.html"
fi

# Substitute BACKEND_URL in nginx config (for proxy if needed)
envsubst '$BACKEND_URL' < /etc/nginx/conf.d/default.conf.template > /etc/nginx/conf.d/default.conf

echo "==> Generated nginx config:"
cat /etc/nginx/conf.d/default.conf

exec nginx -g 'daemon off;'
