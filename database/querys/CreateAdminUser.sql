USE [master]
GO
CREATE LOGIN [user_PP] WITH PASSWORD=N'O4+AX54uv=(h', DEFAULT_DATABASE=[master], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO
USE [ProdigyPlanning]
GO
CREATE USER [user_PP] FOR LOGIN [user_PP]
GO
USE [ProdigyPlanning]
GO
ALTER ROLE [db_datareader] ADD MEMBER [user_PP]
GO
USE [ProdigyPlanning]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [user_PP]
GO
USE [ProdigyPlanning]
GO
ALTER ROLE [db_owner] ADD MEMBER [user_PP]
GO
