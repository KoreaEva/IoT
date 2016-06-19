SELECT * FROM dbo.SensorData

DELETE FROM dbo.SensorData

CREATE TABLE [dbo].[SensorData] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
	DeviceID	  varchar(20)	NOT NULL,
    [Temperature] float           NULL,
    [Humidity]    float           NULL,
    [Dust]        float           NULL,
    [RegiDate]    SMALLDATETIME DEFAULT (getdate()) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);