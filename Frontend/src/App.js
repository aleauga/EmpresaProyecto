import SubscriptionForm from "./components/SubscriptionForm";
import { SignalRProvider } from "./contexts/SignalRContext";
import { v4 as uuidv4 } from 'uuid';

function App() {
  const clientId = uuidv4();
  return (
    <SignalRProvider userId={clientId}>
        <SubscriptionForm userId={clientId} />
    </SignalRProvider>

  );
}

export default App;

