/// Shape returned by GET/POST/PUT /api/equipment and GET/POST /api/equipment-categories.
/// See docs/03-domenemodell.md for the EquipmentStatus state machine.
export type EquipmentStatus = 'Available' | 'OnLoan' | 'OutOfService' | 'WrittenOff';
export type EquipmentCondition = 'New' | 'Good' | 'Worn' | 'Damaged';

export type EquipmentCategory = {
  id: string;
  name: string;
  parentCategoryId: string | null;
  createdByStaffId: string | null;
  updatedByStaffId: string | null;
};

/// Body for POST /api/equipment-categories. Null/omitted parentCategoryId creates a top-level category.
export type CreateEquipmentCategoryRequest = {
  name: string;
  parentCategoryId?: string;
};

/// Body for PUT /api/equipment-categories/{id}.
export type RenameEquipmentCategoryRequest = {
  name: string;
};

export type Equipment = {
  id: string;
  name: string;
  serialNumber: string;
  categoryId: string;
  categoryName: string;
  condition: EquipmentCondition;
  status: EquipmentStatus;
  createdByStaffId: string | null;
  updatedByStaffId: string | null;
};

/// Body for POST /api/equipment.
export type CreateEquipmentRequest = {
  name: string;
  serialNumber: string;
  categoryId: string;
  condition?: EquipmentCondition;
};
