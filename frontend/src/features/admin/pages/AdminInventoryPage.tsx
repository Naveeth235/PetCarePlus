// src/features/admin/pages/AdminInventoryPage.tsx
import React, { useEffect, useState } from "react";
import { fetchInventory, createInventoryItem, updateInventoryItem, deleteInventoryItem, searchInventory, type InventoryItem, type CreateInventoryDto, type UpdateInventoryDto } from "../inventoryApi";
import { Button, Modal, Input, message, Popconfirm, Select, Card as AntdCard, Row, Col, Tag } from "antd";
import { CaretUpFilled, CaretDownFilled } from '@ant-design/icons';

const emptyForm: CreateInventoryDto = { name: "", quantity: 0, category: "", supplier: "", expiryDate: undefined, description: "" };

const categoryColors: Record<string, string> = {
  Medicine: "blue",
  Food: "green",
  Toys: "orange",
  Supplies: "purple",
  Other: "default",
  Tools: "geekblue"
};

// Low stock thresholds
const LOW_STOCK_THRESHOLD = 5;
const CRITICAL_STOCK_THRESHOLD = 3;

const AdminInventoryPage: React.FC = () => {
  const [items, setItems] = useState<InventoryItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<InventoryItem | null>(null);
  const [form, setForm] = useState<CreateInventoryDto | UpdateInventoryDto>(emptyForm);
  const [error, setError] = useState<string | null>(null);
  const [categoryFilter, setCategoryFilter] = useState<string | undefined>(undefined);
  const [formError, setFormError] = useState<string | null>(null);
  const [search, setSearch] = useState("");
  const [searching, setSearching] = useState(false);

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
    setForm({ name: item.name, quantity: item.quantity, category: item.category, supplier: item.supplier, expiryDate: item.expiryDate || undefined, description: item.description || "" });
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

  const handleSearch = async () => {
    if (!search.trim()) {
      setCategoryFilter(undefined);
      load();
      return;
    }
    setSearching(true);
    setError(null);
    const res = await searchInventory(search);
    if (res.ok) {
      setItems(res.data);
      // If only one item is found, set the filter to its category
      if (res.data.length === 1) {
        setCategoryFilter(res.data[0].category);
      } else if (res.data.length > 1) {
        // If all items have the same category, set filter to that category
        const uniqueCategories = Array.from(new Set(res.data.map(i => i.category)));
        if (uniqueCategories.length === 1) {
          setCategoryFilter(uniqueCategories[0]);
        } else {
          setCategoryFilter(undefined);
        }
      } else {
        setCategoryFilter(undefined);
      }
    } else setError(res.detail || "Search failed");
    setSearching(false);
  };

  // Get unique categories from items
  const categories = Array.from(new Set(items.map(i => i.category || "Other")));
  const filterOptions = [{ label: 'All', value: '' }, ...categories.map(c => ({ label: c, value: c }))];
  const filteredItems = categoryFilter ? items.filter(i => i.category === categoryFilter) : items;

  // Low stock items for banners
  const criticalStockItems = items.filter(i => i.quantity > 0 && i.quantity < CRITICAL_STOCK_THRESHOLD);
  const lowStockItems = items.filter(i => i.quantity >= CRITICAL_STOCK_THRESHOLD && i.quantity <= LOW_STOCK_THRESHOLD);

  return (
    <div>
      <h1 style={{ fontSize: 25, fontWeight: 700, padding: '0 0 32px 0', textAlign: 'left' }}>
        Inventory Management
      </h1>
      {/* Critical stock banner */}
      {criticalStockItems.length > 0 && (
        <div style={{
          background: '#fff1f0',
          border: '1px solid #ff4d4f',
          color: '#a8071a',
          padding: 16,
          borderRadius: 8,
          marginBottom: 16,
          fontWeight: 500
        }}>
          🚨 Critical stock alert: {criticalStockItems.length} item{criticalStockItems.length > 1 ? 's are' : ' is'} critically low in stock.
          {criticalStockItems.length <= 5 && (
            <span style={{ marginLeft: 8 }}>
              {criticalStockItems.map(i => i.name).join(', ')}
            </span>
          )}
        </div>
      )}
      {/* Low stock banner */}
      {lowStockItems.length > 0 && (
        <div style={{
          background: '#fffbe6',
          border: '1px solid #ffe58f',
          color: '#ad6800',
          padding: 16,
          borderRadius: 8,
          marginBottom: 16,
          fontWeight: 500
        }}>
          ⚠️ Low stock alert: {lowStockItems.length} item{lowStockItems.length > 1 ? 's are' : ' is'} low in stock.
          {lowStockItems.length <= 5 && (
            <span style={{ marginLeft: 8 }}>
              {lowStockItems.map(i => i.name).join(', ')}
            </span>
          )}
        </div>
      )}
      <div style={{ display: "flex", gap: 40, marginBottom: 16, alignItems: 'center', justifyContent: 'flex-start' }}>
        <Button type="primary" onClick={openCreate}>Add Item</Button>
        <Input.Search
          placeholder="Search inventory..."
          value={search}
          onChange={e => setSearch(e.target.value)}
          onSearch={handleSearch}
          enterButton
          loading={searching}
          style={{ maxWidth: 320, marginLeft: 0 }}
        />
        <Select
          allowClear
          placeholder="Category"
          style={{ minWidth: 180, marginLeft: 0 }}
          value={categoryFilter ?? ''}
          onChange={val => {
            if (!val) {
              setCategoryFilter(undefined);
              setSearch("");
              load(); // Show all products when filter is set to All
            } else {
              setCategoryFilter(val);
            }
          }}
          options={filterOptions}
        />
      </div>
      <Row gutter={[16, 16]}>
        {filteredItems.map(item => (
          <Col xs={24} sm={12} md={8} lg={6} key={item.id}>
            <AntdCard
              style={{ minHeight: 210, display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}
              bodyStyle={{ display: 'flex', flexDirection: 'column', height: '100%' }}
              title={<span>{item.name} <Tag color={categoryColors[item.category] || "default"}>{item.category || "Other"}</Tag>{' '}
                {item.quantity > 0 && item.quantity < CRITICAL_STOCK_THRESHOLD && <Tag color="red">Critical</Tag>}
                {item.quantity >= CRITICAL_STOCK_THRESHOLD && item.quantity <= LOW_STOCK_THRESHOLD && <Tag color="orange">Low Stock</Tag>}
              </span>}
              actions={[
                <Button onClick={() => openEdit(item)} key="edit">Edit</Button>,
                <Popconfirm title="Delete?" onConfirm={() => handleDelete(item.id)} key="delete">
                  <Button danger>Delete</Button>
                </Popconfirm>
              ]}
            >
              <div style={{ flex: 1 }}>
                <p style={{ display: 'flex', alignItems: 'center' }}>
                  <b>Quantity:</b>
                  <Button
                    size="small"
                    icon={<CaretDownFilled style={{ color: '#fa541c', fontSize: 16 }} />}
                    style={{ marginLeft: 8, background: '#fff7e6', border: 'none' }}
                    disabled={item.quantity <= 0}
                    onClick={async () => {
                      if (item.quantity > 0) {
                        await updateInventoryItem(item.id, { ...item, quantity: item.quantity - 1 });
                        load();
                      }
                    }}
                  />
                  <span style={{ minWidth: 32, textAlign: 'center', display: 'inline-block', fontWeight: 600 }}>
                    {item.quantity}
                  </span>
                  <Button
                    size="small"
                    icon={<CaretUpFilled style={{ color: '#1890ff', fontSize: 16 }} />}
                    style={{ marginLeft: 0, background: '#f0f5ff', border: 'none' }}
                    onClick={async () => {
                      await updateInventoryItem(item.id, { ...item, quantity: item.quantity + 1 });
                      load();
                    }}
                  />
                  {item.quantity > 0 && item.quantity < CRITICAL_STOCK_THRESHOLD && <span style={{ color: 'red', fontWeight: 600, marginLeft: 8 }}>(Critical)</span>}
                  {item.quantity >= CRITICAL_STOCK_THRESHOLD && item.quantity <= LOW_STOCK_THRESHOLD && <span style={{ color: 'orange', fontWeight: 600, marginLeft: 8 }}>(Low)</span>}
                </p>
                <p><b>Supplier:</b> {item.supplier}</p>
                {item.expiryDate && (
                  <p><b>Expiry Date:</b> {new Date(item.expiryDate).toLocaleDateString()}</p>
                )}
                {item.description && (
                  <p><b>Description:</b> {item.description}</p>
                )}
              </div>
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
            { value: "Tools", label: "Tools" },
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
        <div style={{ marginBottom: 4, fontWeight: 500 }}>
          Expiry date <span style={{ color: '#888', fontWeight: 400 }}>(optional)</span>
        </div>
        <Input
          type="date"
          value={form.expiryDate ? form.expiryDate.slice(0, 10) : ""}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
            const val = e.target.value;
            setForm(f => ({ ...f, expiryDate: val ? val : null }));
          }}
          style={{ marginBottom: 8 }}
        />
        <Input
          placeholder="Description (optional)"
          value={form.description || ''}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) => setForm(f => ({ ...f, description: e.target.value }))}
          style={{ marginBottom: 8 }}
        />
      </Modal>
      {error && <div style={{ color: "red" }}>{error}</div>}
    </div>
  );
};

export default AdminInventoryPage;
