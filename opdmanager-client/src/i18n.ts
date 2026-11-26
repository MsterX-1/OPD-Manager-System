import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';

// Basic translations (we will expand this)
const resources = {
  en: {
    translation: {
      "Dashboard": "Dashboard",
      "Queue Monitor": "Queue Monitor",
      "Appointments": "Appointments",
      "Patients": "Patients",
      "My Queue": "My Queue",
      "Dept Portal": "Dept Portal",
      "Doctors": "Doctors",
      "Departments": "Departments",
      "Issues": "Issues",
      "Reports": "Reports",
      "Analytics": "Analytics",
      "Users": "Users",
      "Settings": "Settings",
      "Logout": "Logout",
      "Welcome": "Welcome",
      "Total Patients Today": "Total Patients Today",
      "Waiting": "Waiting",
      "In Progress": "In Progress",
      "Completed": "Completed",
      "Doctor Performance Today": "Doctor Performance Today"
    }
  },
  ar: {
    translation: {
      "Dashboard": "لوحة القيادة",
      "Queue Monitor": "مراقبة الطابور",
      "Appointments": "المواعيد",
      "Patients": "المرضى",
      "My Queue": "طابوري",
      "Dept Portal": "بوابة القسم",
      "Doctors": "الأطباء",
      "Departments": "الأقسام",
      "Issues": "المشكلات",
      "Reports": "التقارير",
      "Analytics": "التحليلات",
      "Users": "المستخدمون",
      "Settings": "الإعدادات",
      "Logout": "تسجيل خروج",
      "Welcome": "مرحباً",
      "Total Patients Today": "إجمالي المرضى اليوم",
      "Waiting": "قيد الانتظار",
      "In Progress": "جاري التنفيذ",
      "Completed": "مكتمل",
      "Doctor Performance Today": "أداء الأطباء اليوم"
    }
  }
};

i18n
  .use(initReactI18next)
  .init({
    resources,
    lng: 'en', // default language
    fallbackLng: 'en',
    interpolation: {
      escapeValue: false
    }
  });

export default i18n;
