본 Lab에서는 Device를 만들고 Azure에서 제공하는 IoT 관련 서비스들을 연결해서 하나의 완성된 시나리오를 만들어 보는데 그 목적이 있습니다. 
여기서는 Azure IoT Hub를 기준으로 해서 Device를 관리하고 데이터를 수집하는 과정을 실습해 볼 수 있게 제작되어 있습니다. IoT Hub에서 수집된 데이터는 Azure에서 제공되는 Stream Aanlytics에서 실시간 분석되며 Raw 데이터는 Blob Storage에 저장하고 1분 단위로 분석된 데이터는 SQL Azure에 기록 됩니다. 
 그렇게 모여전 데이터는 최종적으로 Power BI를 통해서 결과를 볼 수 있게 구성 되어 있습니다. 

# Labs

1. 개발 환경 구성 <br>
   ([https://github.com/KoreaEva/IoT/blob/master/Labs/IoT_Hub/1.Prepare_Lab.md](https://github.com/KoreaEva/IoT/blob/master/Labs/IoT_Hub/1.Prepare_Lab.md))
2. IoT Hub Setting.<br>
   ([https://github.com/KoreaEva/IoT/blob/master/Labs/IoT_Hub/2.IoT_Hub_Setting.md](https://github.com/KoreaEva/IoT/blob/master/Labs/IoT_Hub/2.IoT_Hub_Setting.md))
3. Device Programming. .NET Console Version<br>
   ([https://github.com/KoreaEva/IoT/blob/master/Labs/IoT_Hub/3.Device_Programming.md](https://github.com/KoreaEva/IoT/blob/master/Labs/IoT_Hub/3.Device_Programming.md))
3-1. Device Programming. Rasbrian Version<br>
   ([https://github.com/KoreaEva/IoT/blob/master/Labs/IoT_Hub/3-1.Device_Programming%20-%20Raspbrian.md](https://github.com/KoreaEva/IoT/blob/master/Labs/IoT_Hub/3-1.Device_Programming%20-%20Raspbrian.md))
3-2. Device Programming. Python Code 작성하기<br>
    ([https://github.com/KoreaEva/IoT/blob/master/Labs/IoT_Hub/3-2.Device_Programming%20-%20Python.md](https://github.com/KoreaEva/IoT/blob/master/Labs/IoT_Hub/3-2.Device_Programming%20-%20Python.md))
3-3. Device Programming - Cognitive<br>
    ([https://github.com/KoreaEva/IoT/blob/master/Labs/IoT_Hub/3-3.Device_Programming%20-%20Cognitive.md](https://github.com/KoreaEva/IoT/blob/master/Labs/IoT_Hub/3-3.Device_Programming%20-%20Cognitive.md))
4. Stream Analystics 연결.<br>
   ([https://github.com/KoreaEva/IoT/blob/master/Labs/IoT_Hub/4.Stream_Analytics.md](https://github.com/KoreaEva/IoT/blob/master/Labs/IoT_Hub/4.Stream_Analytics.md))
5. SQL Azure에 통계 데이터 전송하기. <br>
   ([https://github.com/KoreaEva/IoT/blob/master/Labs/IoT_Hub/5.SQL_Azure.md](https://github.com/KoreaEva/IoT/blob/master/Labs/IoT_Hub/5.SQL_Azure.md))
6. Power BI로 Dashboard 제작하기.