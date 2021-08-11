netsh interface set interface "Camera" DISABLED
timeout /t 10
netsh interface set interface "Camera" ENABLED
timeout /t 5