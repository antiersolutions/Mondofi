USE [AISLocal]
GO


DECLARE @Seated_Reservations TABLE(IDs VARCHAR(100));
DECLARE	@PartiallySeated_Reservations TABLE(IDs VARCHAR(100));

INSERT INTO @Seated_Reservations
SELECT Reservation.ReservationId AS IDs FROM Reservation WHERE Reservation.StatusId = 10

INSERT INTO @PartiallySeated_Reservations
SELECT Reservation.ReservationId AS IDs FROM Reservation WHERE Reservation.StatusId = 11

--select * from @Seated_Reservations
--select * from @PartiallySeated_Reservations

UPDATE [dbo].[Reservation]
 SET [StatusId] = 11
 WHERE ReservationId in (select * from @Seated_Reservations)

UPDATE [dbo].[Reservation]
 SET [StatusId] = 10
 WHERE ReservationId in (select * from @PartiallySeated_Reservations)

UPDATE [dbo].[Reservation]
 SET [StatusId] = [StatusId] + 1
 WHERE 3 <= [StatusId]  AND [StatusId] <= 19

UPDATE [dbo].[ReservationAudit]
 SET [StatusId] = 11
 WHERE ReservationId in (select * from @Seated_Reservations)

UPDATE [dbo].[ReservationAudit]
 SET [StatusId] = 10
 WHERE ReservationId in (select * from @PartiallySeated_Reservations)

UPDATE [dbo].[ReservationAudit]
 SET [StatusId] = [StatusId] + 1
 WHERE 3 <= [StatusId]  AND [StatusId] <= 19

UPDATE [dbo].[Status]
 SET [StatusName] = N'Partially-Seated'
 WHERE [StatusId] = 10

UPDATE [dbo].[Status]
 SET [StatusName] = N'Seated'
 WHERE [StatusId] = 11

TRUNCATE TABLE [dbo].[Status]

SET IDENTITY_INSERT [dbo].[Status] ON 
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (1, N'Not-confirmed')
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (2, N'Confirmed')
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (3, N'Online-Booking')
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (4, N'Left-message')
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (5, N'No-answer')
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (6, N'Wrong-Number')
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (7, N'Running-Late')
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (8, N'Partially-Arrived')
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (9, N'All-Arrived')
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (10, N'Paged')
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (11, N'Partially-Seated')
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (12, N'Seated')
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (13, N'Appetizer')
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (14, N'Entree')
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (15, N'Dessert')
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (16, N'Table-Cleared')
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (17, N'Check-Dropped')
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (18, N'Check-Paid')
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (19, N'Finished')
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (20, N'Cancelled')
INSERT [dbo].[Status] ([StatusId], [StatusName]) VALUES (21, N'No-show')
SET IDENTITY_INSERT [dbo].[Status] OFF
GO