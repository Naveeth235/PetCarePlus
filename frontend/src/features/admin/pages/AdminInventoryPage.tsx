// src/features/admin/pages/AdminInventoryPage.tsx
import React, { useEffect, useState } from "react";
import { fetchInventory, createInventoryItem, updateInventoryItem, deleteInventoryItem, type InventoryItem, type CreateInventoryDto, type UpdateInventoryDto } from "../inventoryApi";
import { Button, Modal, Input, message, Popconfirm, Select, Card as AntdCard, Row, Col, Tag } from "antd";

const emptyForm: CreateInventoryDto = { name: "", quantity: 0, category: "", supplier: "", expiryDate: "" };

const categoryColors: Record<string, string> = {
  Medicine: "blue",
  Food: "green",
  Toys: "orange",
  Supplies: "purple",
  Other: "default",
};

const AdminInventoryPage: React.FC = () => {
  const [items, setItems] = useState<InventoryItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<InventoryItem | null>(null);
  const [form, setForm] = useState<CreateInventoryDto | UpdateInventoryDto>(emptyForm);
  const [error, setError] = useState<string | null>(null);
  const [categoryFilter, setCategoryFilter] = useState<string | undefined>(undefined);
  const [formError, setFormError] = useState<string | null>(null);

  const load = async () => {
    setLoading(true);
    setError(null);
    const res = await fetchInventory();
    if (res.ok) setItems(res.data);
    else setError(res.detail || "Failed to load inventory");
    setLoading(false);
  };

  useEffect(() => { load(); }, [modalOpen]);

  const openCreate = () => {
    setEditing(null);
    setForm(emptyForm);
    setFormError(null); // Clear form error when opening
    setModalOpen(true);
  };

  const openEdit = (item: InventoryItem) => {
    setEditing(item);
    setForm({ name: item.name, quantity: item.quantity, category: item.category, supplier: item.supplier, expiryDate: item.expiryDate });
    setFormError(null); // Clear form error when opening
    setModalOpen(true);
  };

  const handleOk = async () => {
    setLoading(true);
    setFormError(null);
    if (editing) {
      const res = await updateInventoryItem(editing.id, form as UpdateInventoryDto);
      if (res && (res as any).duplicate) {
        setFormError("An item with this name already exists.");
        setLoading(false);
        return;
      }
      if (res.ok) {
        message.success("Updated");
        setModalOpen(false);
        load();
      } else {
        setFormError("Update failed");
      }
    } else {
      const res = await createInventoryItem(form as CreateInventoryDto);
      const msg = (res as any).message || (res as any).detail || '';
      if ((res && (res as any).duplicate) || msg.toLowerCase().includes('already exists')) {
        setFormError("An item with this name already exists.");
        setLoading(false);
        return;
      }
      if (res.ok) {
        message.success("Created");
        setModalOpen(false);
        load();
      } else {
        setFormError("Create failed");
      }
    }
    setLoading(false);
  };

  const handleDelete = async (id: string) => {
    setLoading(true);
    const res = await deleteInventoryItem(id);
    if (res.ok) {
      message.success("Deleted");
      load();
    } else message.error(res.detail || "Delete failed");
    setLoading(false);
  };

  // Get unique categories from items
  const categories = Array.from(new Set(items.map(i => i.category || "Other")));
  const filteredItems = categoryFilter ? items.filter(i => i.category === categoryFilter) : items;

  return (
    <div>
      <h1 style={{ fontSize: 25, fontWeight: 700, padding: '0 0 32px 0', textAlign: 'left' }}>
        Inventory Management
      </h1>
      <div style={{ display: "flex", gap: 16, marginBottom: 16 }}>
        <Button type="primary" onClick={openCreate}>Add Item</Button>
        <Select
          allowClear
          placeholder="Category"
          style={{ minWidth: 180 }}
          value={categoryFilter}
          onChange={setCategoryFilter}
          options={categories.map(c => ({ label: c, value: c }))}
        />
      </div>
      <Row gutter={[16, 16]}>
        {filteredItems.map(item => (
          <Col xs={24} sm={12} md={8} lg={6} key={item.id}>
            <AntdCard
              title={<span>{item.name} <Tag color={categoryColors[item.category] || "default"}>{item.category || "Other"}</Tag></span>}
              actions={[
                <Button onClick={() => openEdit(item)} key="edit">Edit</Button>,
                <Popconfirm title="Delete?" onConfirm={() => handleDelete(item.id)} key="delete">
                  <Button danger>Delete</Button>
                </Popconfirm>
              ]}
            >
              <p><b>Quantity:</b> {item.quantity}</p>
              <p><b>Supplier:</b> {item.supplier}</p>
              <p><b>Expiry Date:</b> {item.expiryDate ? new Date(item.expiryDate).toLocaleDateString() : "-"}</p>
            </AntdCard>
          </Col>
        ))}
      </Row>
      <Modal
        open={modalOpen}
        title={editing ? "Edit Item" : "Add Item"}
        onOk={handleOk}
        onCancel={() => { setModalOpen(false); setFormError(null); }}
        confirmLoading={loading}
      >
        {formError && <div style={{ color: "red", marginBottom: 8 }}>{formError}</div>}
        <Input
          placeholder="Name"
          value={form.name}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) => setForm(f => ({ ...f, name: e.target.value }))}
          style={{ marginBottom: 8 }}
        />
        <Input
          placeholder="Quantity"
          type="number"
          min={0}
          value={form.quantity === 0 ? '' : form.quantity}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
            const val = Number(e.target.value);
            if (val >= 0 || e.target.value === '') setForm(f => ({ ...f, quantity: val }));
          }}
          style={{ marginBottom: 8 }}
        />
        <Select
          placeholder="Category"
          value={form.category || undefined}
          onChange={(val: string) => setForm(f => ({ ...f, category: val }))}
          style={{ width: "100%", marginBottom: 8 }}
          options={[
            { value: "Medicine", label: "Medicine" },
            { value: "Food", label: "Food" },
            { value: "Toys", label: "Toys" },
            { value: "Supplies", label: "Supplies" },
            { value: "Other", label: "Other" },
          ]}
          allowClear
          showSearch
        />
        <Input
          placeholder="Supplier"
          value={form.supplier}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) => setForm(f => ({ ...f, supplier: e.target.value }))}
          style={{ marginBottom: 8 }}
        />
        <div style={{ marginBottom: 4, fontWeight: 500 }}>Expiry date</div>
        <Input
          type="date"
          value={form.expiryDate ? form.expiryDate.slice(0, 10) : ""}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) => setForm(f => ({ ...f, expiryDate: e.target.value }))}
        />
      </Modal>
      {error && <div style={{ color: "red" }}>{error}</div>}
    </div>
  );
};

export default AdminInventoryPage;
