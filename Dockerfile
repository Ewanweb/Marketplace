FROM nginx:alpine

# Copy compiled Flutter web assets to Nginx web root
COPY build/web /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
