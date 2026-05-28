FROM node:22-alpine AS build
WORKDIR /app

COPY AppIt.Web/package*.json ./
RUN npm install -g npm@11.7.0
RUN npm ci

COPY AppIt.Web/ .
RUN npm run build -- --configuration production

FROM nginx:1.27-alpine AS final
COPY AppIt.Web/nginx/default.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/AppIt.Web/browser /usr/share/nginx/html

EXPOSE 80
