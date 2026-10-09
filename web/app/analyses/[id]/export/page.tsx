import { ExportScreen } from "@/components/export-screen";

export default async function ExportPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  return <ExportScreen id={id} />;
}
