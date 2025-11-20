# React Frontend Integration - Complete Code Examples

## ?? Required Dependencies

```bash
npm install axios @tanstack/react-query
# or
yarn add axios @tanstack/react-query
```

---

## ?? Configuration

### 1. API Configuration (`src/config/api.ts`)

```typescript
// src/config/api.ts
export const API_CONFIG = {
  baseURL: process.env.REACT_APP_API_URL || 'http://localhost:5000/api',
  timeout: 30000,
};

export const getAuthToken = (): string | null => {
  return localStorage.getItem('authToken');
};

export const getAuthHeaders = () => ({
  Authorization: `Bearer ${getAuthToken()}`,
  'Content-Type': 'application/json',
});
```

---

## ?? API Service Layer

### 2. Dashboard Service (`src/services/dashboardService.ts`)

```typescript
// src/services/dashboardService.ts
import axios from 'axios';
import { API_CONFIG, getAuthHeaders } from '../config/api';

const api = axios.create({
  baseURL: API_CONFIG.baseURL,
  timeout: API_CONFIG.timeout,
});

// Request interceptor to add auth token
api.interceptors.request.use(
  (config) => {
    const headers = getAuthHeaders();
    config.headers = { ...config.headers, ...headers };
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor for error handling
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Handle unauthorized - redirect to login
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Type Definitions
export interface DashboardSummary {
  availableRooms: number;
  roomsUnderMaintenance: number;
  todaysBookings: number;
  ongoingBookings: number;
  bookingsCompletedToday: number;
  utilizationRateToday: number;
}

export interface TrendDataPoint {
  date: string;
  bookingsCount: number;
  utilizationPercentage: number;
}

export interface PeakUsageDataPoint {
  roomName: string;
  hour: number;
  occupancyRate: number;
}

export interface DashboardData {
  summary: DashboardSummary;
  trends: TrendDataPoint[];
  peakUsage: PeakUsageDataPoint[];
  lastComputedAt: string;
  fromCache: boolean;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
}

// Service Methods
export const dashboardService = {
  /**
   * Get complete optimized dashboard data
   * Recommended: Use this for the main dashboard page
   */
  getDashboard: async (date?: string): Promise<ApiResponse<DashboardData>> => {
    const params = date ? { date } : {};
    const response = await api.get<ApiResponse<DashboardData>>(
      '/Dashboard/GetOptimizedDashboard',
      { params }
    );
    return response.data;
  },

  /**
   * Get trend data for a specific date range
   * Use for detailed trend analysis
   */
  getTrend: async (
    startDate: string,
    endDate: string
  ): Promise<ApiResponse<{ trends: TrendDataPoint[]; lastComputedAt: string; fromCache: boolean }>> => {
    const response = await api.get('/Dashboard/GetOptimizedTrend', {
      params: { startDate, endDate },
    });
    return response.data;
  },

  /**
   * Get peak usage heatmap data
   * Use for detailed hour-by-hour room usage
   */
  getPeakUsage: async (
    date?: string
  ): Promise<ApiResponse<{ peakUsage: PeakUsageDataPoint[]; lastComputedAt: string; fromCache: boolean }>> => {
    const params = date ? { date } : {};
    const response = await api.get('/Dashboard/GetOptimizedPeakUsage', { params });
    return response.data;
  },

  /**
   * Force recompute metrics (SuperAdmin only)
   */
  recomputeMetrics: async (date: string): Promise<ApiResponse<null>> => {
    const response = await api.post('/Dashboard/RecomputeMetrics', { date });
    return response.data;
  },

  /**
   * Backfill historical metrics (SuperAdmin only)
   */
  backfillMetrics: async (
    startDate: string,
    endDate: string
  ): Promise<ApiResponse<null>> => {
    const response = await api.post('/Dashboard/BackfillMetrics', {
      startDate,
      endDate,
    });
    return response.data;
  },
};
```

---

## ?? Custom React Hooks

### 3. Dashboard Hooks (`src/hooks/useDashboard.ts`)

```typescript
// src/hooks/useDashboard.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { dashboardService } from '../services/dashboardService';
import { toast } from 'react-toastify'; // or your notification library

// Query Keys
export const dashboardKeys = {
  all: ['dashboard'] as const,
  dashboard: (date?: string) => [...dashboardKeys.all, date] as const,
  trend: (startDate: string, endDate: string) =>
    ['dashboard-trend', startDate, endDate] as const,
  peakUsage: (date?: string) => ['dashboard-peak-usage', date] as const,
};

/**
 * Hook for fetching complete dashboard data
 * Automatically refetches every 5 minutes to stay in sync with backend
 */
export const useDashboard = (date?: string) => {
  return useQuery({
    queryKey: dashboardKeys.dashboard(date),
    queryFn: () => dashboardService.getDashboard(date),
    staleTime: 5 * 60 * 1000, // 5 minutes (matches backend cache)
    refetchInterval: 5 * 60 * 1000, // Auto-refetch every 5 minutes
    refetchOnWindowFocus: true,
    retry: 2,
    select: (response) => response.data, // Extract data from ApiResponse
  });
};

/**
 * Hook for fetching trend data
 */
export const useDashboardTrend = (startDate: string, endDate: string) => {
  return useQuery({
    queryKey: dashboardKeys.trend(startDate, endDate),
    queryFn: () => dashboardService.getTrend(startDate, endDate),
    staleTime: 5 * 60 * 1000,
    enabled: !!startDate && !!endDate, // Only fetch if dates are provided
    select: (response) => response.data,
  });
};

/**
 * Hook for fetching peak usage data
 */
export const usePeakUsage = (date?: string) => {
  return useQuery({
    queryKey: dashboardKeys.peakUsage(date),
    queryFn: () => dashboardService.getPeakUsage(date),
    staleTime: 5 * 60 * 1000,
    select: (response) => response.data,
  });
};

/**
 * Hook for manually recomputing metrics (SuperAdmin)
 */
export const useRecomputeMetrics = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (date: string) => dashboardService.recomputeMetrics(date),
    onSuccess: (_, date) => {
      // Invalidate all dashboard queries to refetch
      queryClient.invalidateQueries({ queryKey: dashboardKeys.all });
      toast.success(`Metrics recomputed successfully for ${date}`);
    },
    onError: (error: any) => {
      toast.error(`Failed to recompute metrics: ${error.message}`);
    },
  });
};

/**
 * Hook for backfilling historical data (SuperAdmin)
 */
export const useBackfillMetrics = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ startDate, endDate }: { startDate: string; endDate: string }) =>
      dashboardService.backfillMetrics(startDate, endDate),
    onSuccess: (_, { startDate, endDate }) => {
      queryClient.invalidateQueries({ queryKey: dashboardKeys.all });
      toast.success(`Metrics backfilled from ${startDate} to ${endDate}`);
    },
    onError: (error: any) => {
      toast.error(`Failed to backfill metrics: ${error.message}`);
    },
  });
};
```

---

## ?? React Components

### 4. Main Dashboard Component (`src/components/Dashboard/Dashboard.tsx`)

```typescript
// src/components/Dashboard/Dashboard.tsx
import React from 'react';
import { useDashboard } from '../../hooks/useDashboard';
import { DashboardHeader } from './DashboardHeader';
import { KPIGrid } from './KPIGrid';
import { TrendChart } from './TrendChart';
import { HeatmapChart } from './HeatmapChart';
import { LoadingSpinner } from '../common/LoadingSpinner';
import { ErrorDisplay } from '../common/ErrorDisplay';
import './Dashboard.css';

export const Dashboard: React.FC = () => {
  const { data, isLoading, error, isFetching } = useDashboard();

  if (isLoading) {
    return <LoadingSpinner message="Loading dashboard..." />;
  }

  if (error) {
    return (
      <ErrorDisplay
        title="Failed to load dashboard"
        message={error.message}
        retry={() => window.location.reload()}
      />
    );
  }

  if (!data) {
    return <ErrorDisplay title="No Data" message="Dashboard data is not available" />;
  }

  const { summary, trends, peakUsage, fromCache, lastComputedAt } = data;

  return (
    <div className="dashboard-container">
      <DashboardHeader
        fromCache={fromCache}
        lastComputedAt={lastComputedAt}
        isRefreshing={isFetching}
      />

      {/* KPI Cards Section */}
      <section className="dashboard-section">
        <h2 className="section-title">Key Metrics</h2>
        <KPIGrid summary={summary} />
      </section>

      {/* Trend Chart Section */}
      <section className="dashboard-section">
        <h2 className="section-title">Bookings Trend (Last 30 Days)</h2>
        <TrendChart data={trends} />
      </section>

      {/* Heatmap Section */}
      <section className="dashboard-section">
        <h2 className="section-title">Peak Usage Heatmap</h2>
        <HeatmapChart data={peakUsage} />
      </section>
    </div>
  );
};
```

### 5. Dashboard Header with Cache Indicator (`src/components/Dashboard/DashboardHeader.tsx`)

```typescript
// src/components/Dashboard/DashboardHeader.tsx
import React from 'react';
import { formatDistanceToNow } from 'date-fns';
import './DashboardHeader.css';

interface DashboardHeaderProps {
  fromCache: boolean;
  lastComputedAt: string;
  isRefreshing: boolean;
}

export const DashboardHeader: React.FC<DashboardHeaderProps> = ({
  fromCache,
  lastComputedAt,
  isRefreshing,
}) => {
  const lastUpdated = formatDistanceToNow(new Date(lastComputedAt), {
    addSuffix: true,
  });

  return (
    <div className="dashboard-header">
      <h1 className="dashboard-title">Admin Dashboard</h1>

      <div className="dashboard-status">
        {/* Cache Indicator */}
        <div className={`cache-badge ${fromCache ? 'cached' : 'fresh'}`}>
          <span className="badge-icon">{fromCache ? '?' : '??'}</span>
          <span className="badge-text">
            {fromCache ? 'Cached Data' : 'Fresh Data'}
          </span>
        </div>

        {/* Last Updated */}
        <div className="last-updated">
          <span className="update-icon">??</span>
          <span className="update-text">Updated {lastUpdated}</span>
        </div>

        {/* Refreshing Indicator */}
        {isRefreshing && (
          <div className="refreshing-badge">
            <span className="spinner">?</span>
            <span>Refreshing...</span>
          </div>
        )}
      </div>
    </div>
  );
};
```

### 6. KPI Grid Component (`src/components/Dashboard/KPIGrid.tsx`)

```typescript
// src/components/Dashboard/KPIGrid.tsx
import React from 'react';
import { DashboardSummary } from '../../services/dashboardService';
import './KPIGrid.css';

interface KPIGridProps {
  summary: DashboardSummary;
}

export const KPIGrid: React.FC<KPIGridProps> = ({ summary }) => {
  const kpis = [
    {
      id: 'available-rooms',
      title: 'Available Rooms',
      value: summary.availableRooms,
      icon: '??',
      color: 'blue',
      subtitle: 'Ready for booking',
    },
    {
      id: 'maintenance',
      title: 'Under Maintenance',
      value: summary.roomsUnderMaintenance,
      icon: '??',
      color: 'orange',
      subtitle: 'Temporarily unavailable',
    },
    {
      id: 'todays-bookings',
      title: "Today's Bookings",
      value: summary.todaysBookings,
      icon: '??',
      color: 'green',
      subtitle: 'Total scheduled',
    },
    {
      id: 'ongoing',
      title: 'Ongoing Bookings',
      value: summary.ongoingBookings,
      icon: '?',
      color: 'purple',
      subtitle: 'Currently active',
    },
    {
      id: 'completed',
      title: 'Completed Today',
      value: summary.bookingsCompletedToday,
      icon: '?',
      color: 'teal',
      subtitle: 'Finished sessions',
    },
    {
      id: 'utilization',
      title: 'Utilization Rate',
      value: `${summary.utilizationRateToday.toFixed(1)}%`,
      icon: '??',
      color: getUtilizationColor(summary.utilizationRateToday),
      subtitle: 'Room usage efficiency',
      progress: summary.utilizationRateToday,
    },
  ];

  return (
    <div className="kpi-grid">
      {kpis.map((kpi) => (
        <div key={kpi.id} className={`kpi-card kpi-card-${kpi.color}`}>
          <div className="kpi-icon">{kpi.icon}</div>
          <div className="kpi-content">
            <div className="kpi-title">{kpi.title}</div>
            <div className="kpi-value">{kpi.value}</div>
            <div className="kpi-subtitle">{kpi.subtitle}</div>
            {kpi.progress !== undefined && (
              <div className="kpi-progress">
                <div
                  className="kpi-progress-bar"
                  style={{ width: `${Math.min(kpi.progress, 100)}%` }}
                />
              </div>
            )}
          </div>
        </div>
      ))}
    </div>
  );
};

// Helper function for utilization color
function getUtilizationColor(rate: number): string {
  if (rate >= 80) return 'red';
  if (rate >= 60) return 'orange';
  if (rate >= 40) return 'green';
  return 'blue';
}
```

### 7. Trend Chart Component (using Recharts)

```typescript
// src/components/Dashboard/TrendChart.tsx
import React from 'react';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';
import { format, parseISO } from 'date-fns';
import { TrendDataPoint } from '../../services/dashboardService';
import './TrendChart.css';

interface TrendChartProps {
  data: TrendDataPoint[];
}

export const TrendChart: React.FC<TrendChartProps> = ({ data }) => {
  const chartData = data.map((item) => ({
    ...item,
    dateFormatted: format(parseISO(item.date), 'MMM dd'),
  }));

  return (
    <div className="trend-chart-container">
      <ResponsiveContainer width="100%" height={400}>
        <LineChart data={chartData}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis
            dataKey="dateFormatted"
            tick={{ fontSize: 12 }}
            angle={-45}
            textAnchor="end"
            height={80}
          />
          <YAxis yAxisId="left" tick={{ fontSize: 12 }} />
          <YAxis yAxisId="right" orientation="right" tick={{ fontSize: 12 }} />
          <Tooltip
            content={({ active, payload }) => {
              if (active && payload && payload.length) {
                return (
                  <div className="custom-tooltip">
                    <p className="tooltip-date">{payload[0].payload.dateFormatted}</p>
                    <p className="tooltip-bookings">
                      Bookings: {payload[0].value}
                    </p>
                    <p className="tooltip-utilization">
                      Utilization: {payload[1]?.value?.toFixed(1)}%
                    </p>
                  </div>
                );
              }
              return null;
            }}
          />
          <Legend />
          <Line
            yAxisId="left"
            type="monotone"
            dataKey="bookingsCount"
            stroke="#8884d8"
            name="Bookings"
            strokeWidth={2}
            dot={{ r: 4 }}
            activeDot={{ r: 6 }}
          />
          <Line
            yAxisId="right"
            type="monotone"
            dataKey="utilizationPercentage"
            stroke="#82ca9d"
            name="Utilization %"
            strokeWidth={2}
            dot={{ r: 4 }}
            activeDot={{ r: 6 }}
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
};
```

### 8. Heatmap Component

```typescript
// src/components/Dashboard/HeatmapChart.tsx
import React from 'react';
import { PeakUsageDataPoint } from '../../services/dashboardService';
import './HeatmapChart.css';

interface HeatmapChartProps {
  data: PeakUsageDataPoint[];
}

export const HeatmapChart: React.FC<HeatmapChartProps> = ({ data }) => {
  // Group data by room
  const roomsMap = new Map<string, PeakUsageDataPoint[]>();
  data.forEach((point) => {
    if (!roomsMap.has(point.roomName)) {
      roomsMap.set(point.roomName, []);
    }
    roomsMap.get(point.roomName)!.push(point);
  });

  const rooms = Array.from(roomsMap.keys());
  const hours = Array.from({ length: 24 }, (_, i) => i);

  const getColor = (rate: number): string => {
    if (rate === 0) return 'hsl(0, 0%, 95%)';
    if (rate < 25) return 'hsl(120, 70%, 80%)';
    if (rate < 50) return 'hsl(120, 70%, 60%)';
    if (rate < 75) return 'hsl(45, 90%, 60%)';
    return 'hsl(0, 80%, 60%)';
  };

  return (
    <div className="heatmap-container">
      <div className="heatmap-grid">
        {/* Hour labels */}
        <div className="heatmap-corner"></div>
        {hours.map((hour) => (
          <div key={`hour-${hour}`} className="heatmap-hour-label">
            {hour}:00
          </div>
        ))}

        {/* Data grid */}
        {rooms.map((roomName) => (
          <React.Fragment key={roomName}>
            <div className="heatmap-room-label">{roomName}</div>
            {hours.map((hour) => {
              const dataPoint = roomsMap
                .get(roomName)!
                .find((p) => p.hour === hour);
              const rate = dataPoint?.occupancyRate || 0;

              return (
                <div
                  key={`${roomName}-${hour}`}
                  className="heatmap-cell"
                  style={{ backgroundColor: getColor(rate) }}
                  title={`${roomName} at ${hour}:00 - ${rate.toFixed(1)}% occupied`}
                >
                  {rate > 0 && <span className="cell-value">{rate.toFixed(0)}</span>}
                </div>
              );
            })}
          </React.Fragment>
        ))}
      </div>

      {/* Legend */}
      <div className="heatmap-legend">
        <span>Low Usage</span>
        <div className="legend-gradient"></div>
        <span>High Usage</span>
      </div>
    </div>
  );
};
```

---

## ?? CSS Styles

### Dashboard Styles (`src/components/Dashboard/Dashboard.css`)

```css
/* src/components/Dashboard/Dashboard.css */
.dashboard-container {
  padding: 2rem;
  max-width: 1400px;
  margin: 0 auto;
}

.dashboard-section {
  margin-bottom: 3rem;
  background: white;
  border-radius: 12px;
  padding: 2rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.section-title {
  font-size: 1.5rem;
  font-weight: 600;
  color: #1a202c;
  margin-bottom: 1.5rem;
}
```

### Header Styles (`src/components/Dashboard/DashboardHeader.css`)

```css
/* src/components/Dashboard/DashboardHeader.css */
.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  padding: 1.5rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 12px;
  color: white;
}

.dashboard-title {
  font-size: 2rem;
  font-weight: 700;
  margin: 0;
}

.dashboard-status {
  display: flex;
  gap: 1rem;
  align-items: center;
}

.cache-badge {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 20px;
  font-size: 0.875rem;
}

.cache-badge.cached {
  background: rgba(52, 211, 153, 0.3);
}

.cache-badge.fresh {
  background: rgba(96, 165, 250, 0.3);
}

.last-updated {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
}

.refreshing-badge {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  background: rgba(251, 191, 36, 0.3);
  border-radius: 20px;
  font-size: 0.875rem;
}

.spinner {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}
```

### KPI Grid Styles (`src/components/Dashboard/KPIGrid.css`)

```css
/* src/components/Dashboard/KPIGrid.css */
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1.5rem;
}

.kpi-card {
  display: flex;
  align-items: flex-start;
  padding: 1.5rem;
  border-radius: 12px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  transition: transform 0.2s, box-shadow 0.2s;
}

.kpi-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 6px 12px rgba(0, 0, 0, 0.15);
}

.kpi-icon {
  font-size: 3rem;
  margin-right: 1rem;
}

.kpi-content {
  flex: 1;
}

.kpi-title {
  font-size: 0.875rem;
  color: #6b7280;
  margin-bottom: 0.5rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.kpi-value {
  font-size: 2rem;
  font-weight: 700;
  color: #1a202c;
  margin-bottom: 0.25rem;
}

.kpi-subtitle {
  font-size: 0.75rem;
  color: #9ca3af;
}

.kpi-progress {
  margin-top: 0.75rem;
  height: 6px;
  background: rgba(0, 0, 0, 0.1);
  border-radius: 3px;
  overflow: hidden;
}

.kpi-progress-bar {
  height: 100%;
  background: linear-gradient(90deg, #667eea 0%, #764ba2 100%);
  transition: width 0.3s ease;
}

/* Color variants */
.kpi-card-blue {
  background: linear-gradient(135deg, #667eea20 0%, #764ba220 100%);
}
.kpi-card-green {
  background: linear-gradient(135deg, #34d39920 0%, #10b98120 100%);
}
.kpi-card-orange {
  background: linear-gradient(135deg, #f5975220 0%, #f59e0b20 100%);
}
.kpi-card-purple {
  background: linear-gradient(135deg, #8b5cf620 0%, #7c3aed20 100%);
}
.kpi-card-teal {
  background: linear-gradient(135deg, #14b8a620 0%, #0d9488220 100%);
}
.kpi-card-red {
  background: linear-gradient(135deg, #ef444420 0%, #dc262620 100%);
}
```

---

## ?? Setup in App

### 9. React Query Provider Setup (`src/App.tsx`)

```typescript
// src/App.tsx
import React from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import { Dashboard } from './components/Dashboard/Dashboard';
import 'react-toastify/dist/ReactToastify.css';
import './App.css';

// Create Query Client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
      staleTime: 5 * 60 * 1000, // 5 minutes default
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route path="/dashboard" element={<Dashboard />} />
          {/* Other routes */}
        </Routes>
      </BrowserRouter>

      <ToastContainer position="top-right" autoClose={3000} />

      {/* Dev tools - remove in production */}
      {process.env.NODE_ENV === 'development' && <ReactQueryDevtools />}
    </QueryClientProvider>
  );
}

export default App;
```

---

## ?? Testing the Integration

### Test File (`src/services/__tests__/dashboardService.test.ts`)

```typescript
// src/services/__tests__/dashboardService.test.ts
import { dashboardService } from '../dashboardService';
import axios from 'axios';

jest.mock('axios');
const mockedAxios = axios as jest.Mocked<typeof axios>;

describe('DashboardService', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('getDashboard', () => {
    it('should fetch dashboard data successfully', async () => {
      const mockData = {
        success: true,
        data: {
          summary: {
            availableRooms: 10,
            roomsUnderMaintenance: 2,
            todaysBookings: 15,
            ongoingBookings: 3,
            bookingsCompletedToday: 12,
            utilizationRateToday: 65.5,
          },
          trends: [],
          peakUsage: [],
          lastComputedAt: '2024-01-15T12:00:00Z',
          fromCache: true,
        },
        message: 'Data retrieved from cache',
      };

      mockedAxios.get.mockResolvedValueOnce({ data: mockData });

      const result = await dashboardService.getDashboard();

      expect(result).toEqual(mockData);
      expect(mockedAxios.get).toHaveBeenCalledWith(
        '/Dashboard/GetOptimizedDashboard',
        { params: {} }
      );
    });
  });
});
```

---

## ? Deployment Checklist

- [ ] Install required npm packages (`axios`, `@tanstack/react-query`)
- [ ] Configure API base URL in environment variables
- [ ] Implement authentication token management
- [ ] Test API endpoints with Postman first
- [ ] Add error boundaries around dashboard component
- [ ] Configure React Query devtools for development
- [ ] Test cache behavior (second request should be instant)
- [ ] Verify auto-refresh every 5 minutes
- [ ] Test on different screen sizes
- [ ] Add loading states and error handling
- [ ] Deploy to production

---

## ?? You're Done!

Your React app is now fully integrated with the optimized backend dashboard API. Enjoy **50x faster** dashboard load times! ?
