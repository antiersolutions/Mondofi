USE [AISLocal]
GO

CREATE TABLE [dbo].[MessageTemplate](
	[MessageTemplateId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[BccEmailAddresses] [nvarchar](200) NULL,
	[Subject] [nvarchar](1000) NULL,
	[Body] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.MessageTemplate] PRIMARY KEY CLUSTERED 
(
	[MessageTemplateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

INSERT [dbo].[MessageTemplate] ([Name], [BccEmailAddresses], [Subject], [Body], [IsActive]) 
VALUES (N'Customer.OnlineBookingSucceed', NULL, N'Booking Completed Successfully!!!.', N'<!DOCTYPE html><html xmlns="http://www.w3.org/1999/xhtml"><head><title>Booking Success</title><style type="text/css">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table {margin: 0px;}body, td, p {font-size: 13px;font-family: "Segoe UI", Tahoma, Arial, Helvetica, sans-serif;line-height: 18px;color: #163764;}body {background: #efefef;}p {margin-top: 0px;margin-bottom: 10px;}img {border: 0px;}th {font-weight: bold;color: #ffffff;padding: 5px 0 5px 0;}ul {list-style-type: square;}li {line-height: normal;margin-bottom: 5px;}.template-body {width: 800px;padding: 10px;border: 1px solid #ccc;}</style></head><body><center><table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body"><tbody><tr><td><p><br /><br />Hello %FullName%,<br />Your booking has been completed. Below is the summary of the booking. <br /><br /><b>Reservation Number:</b>%ReservationId%<br /><b>Date booked:</b>%ReservationDate%<br /><b>Time:</b>%TimeForm%<br /><b>Party:</b>%Covers%<br /><br /><br /><br /><br /><b>Edit Url:</b><a target="_blank" href="%EditUrl%">%EditUrl%</a><br /><br /><br /><br /></p></td></tr></tbody></table></center></body></html>', 1)