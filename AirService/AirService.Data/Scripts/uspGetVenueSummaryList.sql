USE [AirService]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[uspGetVenueSummaryList]
AS 
    SELECT  V.Id ,
            V.Title ,
            V.EwayCustomerId AS [eWay Customer Id],
            V.Description ,
            V.Address1 + ' ' + V.Address2 + ', ' + V.Suburb + ', ' + S.Title
            + ', ' + C.Title AS Location ,
            V.CreateDate AS [Date Joined] ,
            V.IsActive AS [Service On/Off] ,
            CASE WHEN V.VenueAccountType = 1 THEN 'Premium'
                 WHEN V.VenueAccountType = 2 THEN 'Lite'
                 ELSE 'Trial'
            END AS [Account Type] ,
            ( SELECT    COUNT(ID)
              FROM      dbo.Orders
              WHERE     VenueId = V.Id
                        AND OrderDate > DATEADD(d, -30, GETDATE())
            ) AS [Num orders in last 30 days] ,
            ( SELECT    COUNT(ID)
                        / CASE WHEN DATEDIFF(m, v.CreateDate, GETDATE()) > 12
                               THEN 12
                               WHEN DATEDIFF(m, v.CreateDate, GETDATE()) = 0
                               THEN 1
                               ELSE DATEDIFF(m, v.CreateDate, GETDATE())
                          END
              FROM      dbo.Orders
              WHERE     VenueId = V.Id
                        AND OrderDate > DATEADD(yy, -1, GETDATE())
            ) AS [Avg orders per month in last 12Months] ,
            ( SELECT    ISNULL(SUM(Price), 0)
              FROM      dbo.OrderItems
                        INNER JOIN dbo.Orders ON dbo.OrderItems.OrderId = dbo.Orders.Id
              WHERE     VenueId = V.Id
                        AND OrderDate > DATEADD(d, -30, GETDATE())
            ) AS [Total value of orders in the last 30 days]
    FROM    dbo.Venues V
            LEFT OUTER JOIN dbo.States S ON V.StateId = S.Id
            LEFT OUTER JOIN dbo.Countries C ON V.CountryId = C.Id
    ORDER BY V.CreateDate DESC 