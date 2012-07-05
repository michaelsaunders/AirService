USE [AirService]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[uspGetStatistics]
AS 
    SELECT  ( SELECT    COUNT(Id)
              FROM      dbo.Venues
              WHERE     Status = 1
                        AND ( VenueAccountType = 1
                              OR VenueAccountType = 2
                            )
            ) AS [Active Subscription] ,
            ( SELECT DISTINCT
                        COUNT(Venues.Id)
              FROM      dbo.Venues
                        INNER JOIN dbo.ServiceProviders ON dbo.Venues.Id = dbo.ServiceProviders.VenueId
                        INNER JOIN dbo.aspnet_Membership M ON dbo.ServiceProviders.UserId = M.UserId
              WHERE     M.IsApproved = 1
            ) AS [Total Registration in last 30 days]
	