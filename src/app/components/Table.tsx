"use client";
import { useState } from "react";

interface RowData {
    id: number;
    name: string;
}

interface TableProps {
    data: RowData[];
    ondelete: (id: number) => void;
    onupdate: (id: number, updatedData: RowData) => void;
}

export default function Table({ data, ondelete, onupdate }: TableProps) {
    const [editId, setEditId] = useState<number | null>(null);
    const [editData, setEditData] = useState<RowData>({ id: 0, name: '' });

    const handleEdit = (id: number, row: RowData) => {
        setEditId(id);
        setEditData(row);
    };

    const handleSave = () => { 
        onupdate(editId as number, editData);
        setEditId(null);
    };

    return (
        <table>
            <thead>
                <tr>
                    <th style={{ padding: '0 10px' }}>Id</th>
                    <th style={{ padding: '0 10px' }}>Name</th>
                    <th style={{ padding: '0 10px' }}>Action</th>

                </tr>
            </thead>
            <tbody>
                {data.map((row) => (
                    <tr key={row.id}>
                        <td>{row.id}</td>
                        <td>
                            {editId === row.id ? (
                                <input
                                    type="text"
                                    value={editData.name}
                                    onChange={(e) => setEditData({ ...editData, name: e.target.value })}
                                />
                            ) : (
                                row.name
                            )}
                        </td>
                        <td>
                            {editId === row.id ? (
                                <button onClick={() => handleSave()}>Save</button>
                            ) : (
                                <>
                                    <button onClick={() => handleEdit(row.id, row)}>Edit</button>
                                    <button onClick={() => ondelete(row.id)}>Delete</button>
                                </>
                            )}
                        </td>
                    </tr>
                ))}
            </tbody>
        </table>
    );
}
