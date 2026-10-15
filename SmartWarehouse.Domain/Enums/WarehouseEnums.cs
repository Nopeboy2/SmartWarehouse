namespace SmartWarehouse.Domain.Enums;

public enum RfidTagStatus { Active = 1, Lost = 2, Retired = 3 }
public enum OrderStatus { Draft = 1, Released = 2, InProgress = 3, Completed = 4, Cancelled = 5, Failed = 6 }
public enum MovementType { Receipt = 1, Reservation = 2, ReleaseReservation = 3, Dispatch = 4, Transfer = 5, Adjustment = 6 }
public enum DeviceType { RfidReader = 1, Conveyor = 2, Agv = 3 }
public enum DeviceStatus { Offline = 1, Idle = 2, Running = 3, Error = 4, EmergencyStop = 5 }
public enum DeviceTaskStatus { Queued = 1, Assigned = 2, InProgress = 3, Completed = 4, Failed = 5, Cancelled = 6 }

