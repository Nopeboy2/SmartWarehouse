IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AGVTasks] (
    [Id] uniqueidentifier NOT NULL,
    [TaskNumber] nvarchar(48) NOT NULL,
    [Status] int NOT NULL,
    [PickupPoint] nvarchar(max) NULL,
    [DropoffPoint] nvarchar(max) NULL,
    [BatteryPercent] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_AGVTasks] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_AGVTasks_BatteryPercent] CHECK ([BatteryPercent] BETWEEN 0 AND 100)
);

CREATE TABLE [AspNetRoles] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] uniqueidentifier NOT NULL,
    [DisplayName] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [Categories] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(32) NOT NULL,
    [Name] nvarchar(120) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
);

CREATE TABLE [ConveyorTasks] (
    [Id] uniqueidentifier NOT NULL,
    [TaskNumber] nvarchar(48) NOT NULL,
    [Status] int NOT NULL,
    [SourcePoint] nvarchar(max) NULL,
    [DestinationPoint] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_ConveyorTasks] PRIMARY KEY ([Id])
);

CREATE TABLE [DeviceAlerts] (
    [Id] uniqueidentifier NOT NULL,
    [DeviceCode] nvarchar(40) NOT NULL,
    [Message] nvarchar(400) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [ResolvedAt] datetimeoffset NULL,
    CONSTRAINT [PK_DeviceAlerts] PRIMARY KEY ([Id])
);

CREATE TABLE [Devices] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(40) NOT NULL,
    [Name] nvarchar(120) NOT NULL,
    [Type] int NOT NULL,
    [Status] int NOT NULL,
    [LastSeenAt] datetimeoffset NULL,
    CONSTRAINT [PK_Devices] PRIMARY KEY ([Id])
);

CREATE TABLE [InboundOrders] (
    [Id] uniqueidentifier NOT NULL,
    [Number] nvarchar(40) NOT NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_InboundOrders] PRIMARY KEY ([Id])
);

CREATE TABLE [InventoryMovements] (
    [Id] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [FromLocationId] uniqueidentifier NULL,
    [ToLocationId] uniqueidentifier NULL,
    [Quantity] int NOT NULL,
    [Type] int NOT NULL,
    [ReferenceNumber] nvarchar(64) NOT NULL,
    [IdempotencyKey] nvarchar(128) NOT NULL,
    [OccurredAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_InventoryMovements] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_InventoryMovements_Quantity] CHECK ([Quantity] > 0)
);

CREATE TABLE [OutboundOrders] (
    [Id] uniqueidentifier NOT NULL,
    [Number] nvarchar(40) NOT NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_OutboundOrders] PRIMARY KEY ([Id])
);

CREATE TABLE [RFIDReadEvents] (
    [Id] uniqueidentifier NOT NULL,
    [EventKey] nvarchar(128) NOT NULL,
    [Epc] nvarchar(64) NOT NULL,
    [ReaderCode] nvarchar(40) NOT NULL,
    [ReadPoint] nvarchar(40) NOT NULL,
    [ReadAt] datetimeoffset NOT NULL,
    [IsAccepted] bit NOT NULL,
    [RejectionReason] nvarchar(240) NULL,
    CONSTRAINT [PK_RFIDReadEvents] PRIMARY KEY ([Id])
);

CREATE TABLE [Warehouses] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(32) NOT NULL,
    [Name] nvarchar(160) NOT NULL,
    [Address] nvarchar(max) NULL,
    CONSTRAINT [PK_Warehouses] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] uniqueidentifier NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Products] (
    [Id] uniqueidentifier NOT NULL,
    [Sku] nvarchar(64) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NULL,
    [UnitOfMeasure] nvarchar(16) NOT NULL,
    [CategoryId] uniqueidentifier NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [WarehouseZones] (
    [Id] uniqueidentifier NOT NULL,
    [WarehouseId] uniqueidentifier NOT NULL,
    [Code] nvarchar(32) NOT NULL,
    [Name] nvarchar(120) NOT NULL,
    CONSTRAINT [PK_WarehouseZones] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WarehouseZones_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [InboundOrderDetails] (
    [Id] uniqueidentifier NOT NULL,
    [InboundOrderId] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [ExpectedQuantity] int NOT NULL,
    [ReceivedQuantity] int NOT NULL,
    CONSTRAINT [PK_InboundOrderDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_InboundOrderDetails_Quantities] CHECK ([ExpectedQuantity] > 0 AND [ReceivedQuantity] >= 0 AND [ReceivedQuantity] <= [ExpectedQuantity]),
    CONSTRAINT [FK_InboundOrderDetails_InboundOrders_InboundOrderId] FOREIGN KEY ([InboundOrderId]) REFERENCES [InboundOrders] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_InboundOrderDetails_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [RfidTags] (
    [Id] uniqueidentifier NOT NULL,
    [Epc] nvarchar(64) NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [Status] int NOT NULL,
    [LastSeenAt] datetimeoffset NULL,
    CONSTRAINT [PK_RfidTags] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RfidTags_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [WarehouseLocations] (
    [Id] uniqueidentifier NOT NULL,
    [ZoneId] uniqueidentifier NOT NULL,
    [Code] nvarchar(40) NOT NULL,
    [Name] nvarchar(120) NOT NULL,
    [CapacityUnits] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_WarehouseLocations] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_WarehouseLocations_CapacityUnits] CHECK ([CapacityUnits] > 0),
    CONSTRAINT [FK_WarehouseLocations_WarehouseZones_ZoneId] FOREIGN KEY ([ZoneId]) REFERENCES [WarehouseZones] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Inventory] (
    [Id] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [LocationId] uniqueidentifier NOT NULL,
    [QuantityOnHand] int NOT NULL,
    [QuantityReserved] int NOT NULL,
    [QuantityInTransit] int NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Inventory] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Inventory_Quantities] CHECK ([QuantityOnHand] >= 0 AND [QuantityReserved] >= 0 AND [QuantityInTransit] >= 0),
    CONSTRAINT [CK_Inventory_Reserved] CHECK ([QuantityReserved] <= [QuantityOnHand]),
    CONSTRAINT [FK_Inventory_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Inventory_WarehouseLocations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [WarehouseLocations] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [OutboundOrderDetails] (
    [Id] uniqueidentifier NOT NULL,
    [OutboundOrderId] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [LocationId] uniqueidentifier NULL,
    [Quantity] int NOT NULL,
    [PickedQuantity] int NOT NULL,
    CONSTRAINT [PK_OutboundOrderDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_OutboundOrderDetails_Quantities] CHECK ([Quantity] > 0 AND [PickedQuantity] >= 0 AND [PickedQuantity] <= [Quantity]),
    CONSTRAINT [FK_OutboundOrderDetails_OutboundOrders_OutboundOrderId] FOREIGN KEY ([OutboundOrderId]) REFERENCES [OutboundOrders] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_OutboundOrderDetails_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_OutboundOrderDetails_WarehouseLocations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [WarehouseLocations] ([Id]) ON DELETE NO ACTION
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[Categories]'))
    SET IDENTITY_INSERT [Categories] ON;
INSERT INTO [Categories] ([Id], [Code], [IsActive], [Name])
VALUES ('10000000-0000-0000-0000-000000000001', N'GENERAL', CAST(1 AS bit), N'Hàng hóa chung');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[Categories]'))
    SET IDENTITY_INSERT [Categories] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'LastSeenAt', N'Name', N'Status', N'Type') AND [object_id] = OBJECT_ID(N'[Devices]'))
    SET IDENTITY_INSERT [Devices] ON;
INSERT INTO [Devices] ([Id], [Code], [LastSeenAt], [Name], [Status], [Type])
VALUES ('70000000-0000-0000-0000-000000000001', N'RFID-IN-01', NULL, N'Cổng RFID nhập', 2, 1),
('70000000-0000-0000-0000-000000000002', N'CONV-01', NULL, N'Băng tải chính', 2, 2),
('70000000-0000-0000-0000-000000000003', N'AGV-01', NULL, N'Xe tự hành 01', 2, 3);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'LastSeenAt', N'Name', N'Status', N'Type') AND [object_id] = OBJECT_ID(N'[Devices]'))
    SET IDENTITY_INSERT [Devices] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Address', N'Code', N'Name') AND [object_id] = OBJECT_ID(N'[Warehouses]'))
    SET IDENTITY_INSERT [Warehouses] ON;
INSERT INTO [Warehouses] ([Id], [Address], [Code], [Name])
VALUES ('30000000-0000-0000-0000-000000000001', N'Khu vực demo', N'WH-MAIN', N'Kho chính');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Address', N'Code', N'Name') AND [object_id] = OBJECT_ID(N'[Warehouses]'))
    SET IDENTITY_INSERT [Warehouses] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CategoryId', N'Description', N'IsActive', N'Name', N'Sku', N'UnitOfMeasure') AND [object_id] = OBJECT_ID(N'[Products]'))
    SET IDENTITY_INSERT [Products] ON;
INSERT INTO [Products] ([Id], [CategoryId], [Description], [IsActive], [Name], [Sku], [UnitOfMeasure])
VALUES ('20000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', NULL, CAST(1 AS bit), N'Thùng hàng mẫu', N'DEMO-001', N'THÙNG');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CategoryId', N'Description', N'IsActive', N'Name', N'Sku', N'UnitOfMeasure') AND [object_id] = OBJECT_ID(N'[Products]'))
    SET IDENTITY_INSERT [Products] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'Name', N'WarehouseId') AND [object_id] = OBJECT_ID(N'[WarehouseZones]'))
    SET IDENTITY_INSERT [WarehouseZones] ON;
INSERT INTO [WarehouseZones] ([Id], [Code], [Name], [WarehouseId])
VALUES ('40000000-0000-0000-0000-000000000001', N'A', N'Khu kệ A', '30000000-0000-0000-0000-000000000001');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'Name', N'WarehouseId') AND [object_id] = OBJECT_ID(N'[WarehouseZones]'))
    SET IDENTITY_INSERT [WarehouseZones] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CapacityUnits', N'Code', N'IsActive', N'Name', N'ZoneId') AND [object_id] = OBJECT_ID(N'[WarehouseLocations]'))
    SET IDENTITY_INSERT [WarehouseLocations] ON;
INSERT INTO [WarehouseLocations] ([Id], [CapacityUnits], [Code], [IsActive], [Name], [ZoneId])
VALUES ('50000000-0000-0000-0000-000000000001', 100, N'A-01-01', CAST(1 AS bit), N'Ô kệ A-01-01', '40000000-0000-0000-0000-000000000001');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CapacityUnits', N'Code', N'IsActive', N'Name', N'ZoneId') AND [object_id] = OBJECT_ID(N'[WarehouseLocations]'))
    SET IDENTITY_INSERT [WarehouseLocations] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'LocationId', N'ProductId', N'QuantityInTransit', N'QuantityOnHand', N'QuantityReserved') AND [object_id] = OBJECT_ID(N'[Inventory]'))
    SET IDENTITY_INSERT [Inventory] ON;
INSERT INTO [Inventory] ([Id], [LocationId], [ProductId], [QuantityInTransit], [QuantityOnHand], [QuantityReserved])
VALUES ('60000000-0000-0000-0000-000000000001', '50000000-0000-0000-0000-000000000001', '20000000-0000-0000-0000-000000000001', 0, 24, 0);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'LocationId', N'ProductId', N'QuantityInTransit', N'QuantityOnHand', N'QuantityReserved') AND [object_id] = OBJECT_ID(N'[Inventory]'))
    SET IDENTITY_INSERT [Inventory] OFF;

CREATE UNIQUE INDEX [IX_AGVTasks_TaskNumber] ON [AGVTasks] ([TaskNumber]);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE UNIQUE INDEX [IX_Categories_Code] ON [Categories] ([Code]);

CREATE UNIQUE INDEX [IX_ConveyorTasks_TaskNumber] ON [ConveyorTasks] ([TaskNumber]);

CREATE INDEX [IX_DeviceAlerts_ResolvedAt_CreatedAt] ON [DeviceAlerts] ([ResolvedAt], [CreatedAt]);

CREATE UNIQUE INDEX [IX_Devices_Code] ON [Devices] ([Code]);

CREATE INDEX [IX_InboundOrderDetails_InboundOrderId] ON [InboundOrderDetails] ([InboundOrderId]);

CREATE INDEX [IX_InboundOrderDetails_ProductId] ON [InboundOrderDetails] ([ProductId]);

CREATE UNIQUE INDEX [IX_InboundOrders_Number] ON [InboundOrders] ([Number]);

CREATE INDEX [IX_Inventory_LocationId] ON [Inventory] ([LocationId]);

CREATE UNIQUE INDEX [IX_Inventory_ProductId_LocationId] ON [Inventory] ([ProductId], [LocationId]);

CREATE UNIQUE INDEX [IX_InventoryMovements_IdempotencyKey] ON [InventoryMovements] ([IdempotencyKey]);

CREATE INDEX [IX_InventoryMovements_ProductId_OccurredAt] ON [InventoryMovements] ([ProductId], [OccurredAt]);

CREATE INDEX [IX_OutboundOrderDetails_LocationId] ON [OutboundOrderDetails] ([LocationId]);

CREATE INDEX [IX_OutboundOrderDetails_OutboundOrderId] ON [OutboundOrderDetails] ([OutboundOrderId]);

CREATE INDEX [IX_OutboundOrderDetails_ProductId] ON [OutboundOrderDetails] ([ProductId]);

CREATE UNIQUE INDEX [IX_OutboundOrders_Number] ON [OutboundOrders] ([Number]);

CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);

CREATE UNIQUE INDEX [IX_Products_Sku] ON [Products] ([Sku]);

CREATE INDEX [IX_RFIDReadEvents_Epc_ReadAt] ON [RFIDReadEvents] ([Epc], [ReadAt]);

CREATE UNIQUE INDEX [IX_RFIDReadEvents_EventKey] ON [RFIDReadEvents] ([EventKey]);

CREATE UNIQUE INDEX [IX_RfidTags_Epc] ON [RfidTags] ([Epc]);

CREATE INDEX [IX_RfidTags_ProductId] ON [RfidTags] ([ProductId]);

CREATE UNIQUE INDEX [IX_WarehouseLocations_ZoneId_Code] ON [WarehouseLocations] ([ZoneId], [Code]);

CREATE UNIQUE INDEX [IX_Warehouses_Code] ON [Warehouses] ([Code]);

CREATE UNIQUE INDEX [IX_WarehouseZones_WarehouseId_Code] ON [WarehouseZones] ([WarehouseId], [Code]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261015064027_InitialCreate', N'10.0.13');

COMMIT;
GO

BEGIN TRANSACTION;
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'LastSeenAt', N'Name', N'Status', N'Type') AND [object_id] = OBJECT_ID(N'[Devices]'))
    SET IDENTITY_INSERT [Devices] ON;
INSERT INTO [Devices] ([Id], [Code], [LastSeenAt], [Name], [Status], [Type])
VALUES ('70000000-0000-0000-0000-000000000004', N'RFID-OUT-01', NULL, N'Cổng RFID xuất', 2, 1);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'LastSeenAt', N'Name', N'Status', N'Type') AND [object_id] = OBJECT_ID(N'[Devices]'))
    SET IDENTITY_INSERT [Devices] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Epc', N'LastSeenAt', N'ProductId', N'Status') AND [object_id] = OBJECT_ID(N'[RfidTags]'))
    SET IDENTITY_INSERT [RfidTags] ON;
INSERT INTO [RfidTags] ([Id], [Epc], [LastSeenAt], [ProductId], [Status])
VALUES ('80000000-0000-0000-0000-000000000001', N'E2806894A000000000000001', NULL, '20000000-0000-0000-0000-000000000001', 1);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Epc', N'LastSeenAt', N'ProductId', N'Status') AND [object_id] = OBJECT_ID(N'[RfidTags]'))
    SET IDENTITY_INSERT [RfidTags] OFF;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261015065302_AddDemoRfidTag', N'10.0.13');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [DeviceEvents] (
    [Id] uniqueidentifier NOT NULL,
    [DeviceCode] nvarchar(40) NOT NULL,
    [EventType] nvarchar(40) NOT NULL,
    [Message] nvarchar(400) NOT NULL,
    [CorrelationId] nvarchar(64) NULL,
    [OccurredAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_DeviceEvents] PRIMARY KEY ([Id])
);

CREATE INDEX [IX_InventoryMovements_FromLocationId] ON [InventoryMovements] ([FromLocationId]);

CREATE INDEX [IX_InventoryMovements_ToLocationId] ON [InventoryMovements] ([ToLocationId]);

CREATE INDEX [IX_DeviceEvents_DeviceCode_OccurredAt] ON [DeviceEvents] ([DeviceCode], [OccurredAt]);

ALTER TABLE [InventoryMovements] ADD CONSTRAINT [FK_InventoryMovements_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [InventoryMovements] ADD CONSTRAINT [FK_InventoryMovements_WarehouseLocations_FromLocationId] FOREIGN KEY ([FromLocationId]) REFERENCES [WarehouseLocations] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [InventoryMovements] ADD CONSTRAINT [FK_InventoryMovements_WarehouseLocations_ToLocationId] FOREIGN KEY ([ToLocationId]) REFERENCES [WarehouseLocations] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261015070111_AddDeviceEvents', N'10.0.13');

COMMIT;
GO

