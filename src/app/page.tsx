"use client";
import { useState } from "react";
import Table from "./components/Table"; // Adjust the import path as necessary

interface RecordData {
    id: number;
    name: string;
}

export default function Home() {
    const [records, setRecords] = useState<RecordData[]>([]);
    const [newRecord, setNewRecord] = useState<RecordData>({ id: 0, name: '' });

    const addRecord = () => {
        setRecords([...records, { ...newRecord, id: records.length + 1 }]);
        setNewRecord({ id: 0, name: '' }); // Reset input fields
    };

    const updateRecord = (id: number, updatedData: RecordData) => {
        setRecords(records.map(record => (record.id === id ? updatedData : record)));
    };

    const deleteRecord = (id: number) => {
        setRecords(records.filter(record => record.id !== id));
    };

    return (
        <div className="grid grid-rows-[20px_1fr_20px] items-center justify-items-center min-h-screen p-8 pb-20 gap-16 sm:p-20 font-[family-name:var(--font-geist-sans)]">
            <main className="flex flex-col gap-8 row-start-2 items-center sm:items-start">
                <h1>Add New Record</h1>
                    <input
                        type="text"
                        value={newRecord.name}
                        onChange={(e) => setNewRecord({ ...newRecord, name: e.target.value })}
                        placeholder="Enter name"
                        style={{ color: 'black' }} // Set input text color to black
                    />

                <button onClick={addRecord}>Add Record</button>

                <Table data={records} ondelete={deleteRecord} onupdate={updateRecord} />
            </main>
            <footer className="row-start-3 flex gap-6 flex-wrap items-center justify-center">
                {/* Footer content remains unchanged */}
            </footer>
        </div>
    );
}
