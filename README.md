# finetail
A tail command for Windows with filters, search, text highlight and file rolling...

> finetail.exe -i -f -c cyan "\[.*?\]" -c green INFO -c yellow WARN -c red ERROR E:\Projects\finetail\\\*.log

![image](https://user-images.githubusercontent.com/14943271/219797849-58bef1fc-473d-409e-87c7-95477513548f.png)

dummy.log:
```
2023-02-17 22:01:01 - INFO  - Controller   - GET[/api/status] User[Alf] Port[1234]
2023-02-17 22:01:02 - INFO  - Controller   - GET[/api/status] User[Bob] Port[2345]
2023-02-17 22:01:03 - WARN  - Controller   - Unknown user !
2023-02-17 22:01:04 - INFO  - OrderManager - POST[/api/orders] User[Charles] Command[Send] OrderId[42]
2023-02-17 22:01:05 - ERROR - OrderManager - Failed to send order ! 
2023-02-17 22:01:06 - INFO  - OrderManager - GET[/api/orders]  User[Charles]
2023-02-17 22:01:06 - DEBUG - OrderManager - Search orders...
2023-02-17 22:32:06 - DEBUG - Main - Uptime[527]
```
