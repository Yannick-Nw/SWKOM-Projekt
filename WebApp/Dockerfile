FROM nginx:alpine

# Copy custom nginx.conf
COPY nginx.conf /etc/nginx/nginx.conf

# Copy website content
COPY ./html /app/wwwroot

# Expose port 80
EXPOSE 80

# Start Nginx
CMD ["nginx", "-g", "daemon off;"]