# Stage 1: Build the Angular application
FROM node:20-alpine AS build
WORKDIR /app

# Copy package.json and package-lock.json and install dependencies
COPY package.json package-lock.json ./
RUN npm install

# Copy the rest of the source code
COPY . .

# Build the application
# Use a production build and note the output directory (usually 'dist/{project-name}' or just 'dist')
# Adjust the command if your project name is different or if 'ng build' is sufficient.
# Assuming the output is in 'dist/sotagui'
RUN npm run build -- --output-path=./dist/sotagui

# Stage 2: Serve the application with NGINX
FROM nginx:alpine AS final
# Copy the built files from the build stage into the NGINX serving directory
# Make sure this path matches the output path from your build stage (e.g., /app/dist/sotagui)
COPY --from=build /app/dist/sotagui /usr/share/nginx/html

# Copy a custom NGINX configuration (optional, but good practice for SPAs)
# If you need custom config for routing, create a file named 'nginx.conf' in sotagui
# COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
# The default NGINX ENTRYPOINT runs the server