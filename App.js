import React, { useState, useEffect, useRef } from 'react';
import { 
  StyleSheet, 
  Text, 
  View, 
  TouchableOpacity, 
  TextInput, 
  ScrollView, 
  Animated, 
  Dimensions 
} from 'react-native';

const { width } = Dimensions.get('window');

export default function App() {
  // حالات شاشة الترحيب والواجهة الرئيسية
  const [isSplash, setIsSplash] = useState(true);
  const [currentTab, setCurrentTab] = useState('reminder');
  
  // شريط التقدم والتأثيرات الحركية
  const progressAnim = useRef(new Animated.Value(0)).current;
  const fadeAnim = useRef(new Animated.Value(1)).current;

  // منطق السبحة واللعبة والتنبيهات
  const [tasbeehCount, setTasbeehCount] = useState(0);
  const [currentZikrIndex, setCurrentZikrIndex] = useState(0);
  const [userAnswer, setUserAnswer] = useState('');
  const [gameScore, setGameScore] = useState(0);
  const [question, setQuestion] = useState({ num1: 5, num2: 3, op: '+', answer: 8 });
  const [reminderText, setReminderText] = useState('');

  const azkarAr = [
    "☀️ [أذكار الصباح]: أَصْبَحْنَا وَأَصْبَحَ الْمُلْكُ لِلَّهِ وَالْحَمْدُ لِلَّهِ",
    "🌙 [أذكار المساء]: أَمْسَيْنَا وَأَمْسَى الْمُلْكُ لِلَّهِ وَالْحَمْدُ لِلَّهِ",
    "سُبْحَانَ اللَّهِ وَبِحَمْدِهِ ، سُبْحَانَ اللَّهِ الْعَظِيمِ",
    "أَسْتَغْفِرُ اللَّهَ الْعَظِيمَ وَأَتُوبُ إِلَيْهِ",
    "اللَّهُمَّ صَلِّ وَسَلِّمْ عَلَى نَبِيِّنَا مُحَمَّدٍ",
    "لَا حَوْلَ وَلَا قُوَّةَ إِلَّا بِاللَّهِ الْعَلِيِّ الْعَظِيمِ"
  ];

  useEffect(() => {
    // تشغيل شريط التقدم لمدة 5 ثوانٍ (5000 جزء من الثانية)
    Animated.timing(progressAnim, {
      toValue: 1,
      duration: 4500,
      useNativeDriver: false,
    }).start(() => {
      // اختفاء تدريجي ناعم بعد انتهاء الشريط
      Animated.timing(fadeAnim, {
        toValue: 0,
        duration: 500,
        useNativeDriver: true,
      }).start(() => {
        setIsSplash(false);
      });
    });
    generateQuestion();
  }, []);

  const generateQuestion = () => {
    const num1 = Math.floor(Math.random() * 10) + 1;
    const num2 = Math.floor(Math.random() * 10) + 1;
    const ops = ['+', '-'];
    const op = ops[Math.floor(Math.random() * ops.length)];
    const answer = op === '+' ? num1 + num2 : num1 - num2;
    setQuestion({ num1, num2, op, answer });
    setUserAnswer('');
  };

  const checkAnswer = () => {
    if (parseInt(userAnswer) === question.answer) {
      setGameScore(gameScore + 10);
    }
    generateQuestion();
  };

  // شاشة الترحيب (Splash Screen) المماثلة للصورة تماماً
  if (isSplash) {
    const barWidth = progressAnim.interpolate({
      inputRange: [0, 1],
      outputRange: ['0%', '100%']
    });

    return (
      <Animated.View style={[styles.splashContainer, { opacity: fadeAnim }]}> 
        <Text style={styles.splashText}>WELCOME</Text>
        <View style={styles.pnlTrack}>
          <Animated.View style={[styles.pnlProgress, { width: barWidth }]} />
        </View>
      </Animated.View>
    );
  }

  // واجهة التطبيق الفخمة والحديثة (كأنها موقع ويب فخم)
  return (
    <View style={styles.container}>
      {/* الهيدر العلوي وشريط التنقل للموقع */}
      <View style={styles.navbar}>
        <Text style={styles.navLogo}>AinJamel</Text>
        <View style={styles.navLinks}>
          <TouchableOpacity 
            style={[styles.navBtn, currentTab === 'reminder' && styles.activeNavBtn]} 
            onPress={() => setCurrentTab('reminder')}
          >
            <Text style={[styles.navBtnText, currentTab === 'reminder' && styles.activeNavBtnText]}>التذكير والأذكار</Text>
          </TouchableOpacity>
          <TouchableOpacity 
            style={[styles.navBtn, currentTab === 'tasbeeh' && styles.activeNavBtn]} 
            onPress={() => setCurrentTab('tasbeeh')}
          >
            <Text style={[styles.navBtnText, currentTab === 'tasbeeh' && styles.activeNavBtnText]}>السبحة الرقمية</Text>
          </TouchableOpacity>
          <TouchableOpacity 
            style={[styles.navBtn, currentTab === 'game' && styles.activeNavBtn]} 
            onPress={() => setCurrentTab('game')}
          >
            <Text style={[styles.navBtnText, currentTab === 'game' && styles.activeNavBtnText]}>واحة التحدي</Text>
          </TouchableOpacity>
        </View>
      </View>

      {/* محتوى الصفحة الرئيسي */}
      <ScrollView contentContainerStyle={styles.contentWrapper}>
        {currentTab === 'reminder' && (
          <View style={styles.card}>
            <Text style={styles.cardTitle}>مذكر كبار السن الذكي</Text>
            <TextInput 
              style={styles.input} 
              placeholder="اكتب التذكير الصحي هنا..." 
              placeholderTextColor="#666"
              value={reminderText}
              onChangeText={setReminderText}
            />
            <TouchableOpacity style={styles.primaryButton}>
              <Text style={styles.buttonText}>تفعيل التنبيه الدوري</Text>
            </TouchableOpacity>

            <View style={styles.divider} />

            <Text style={styles.cardSubTitle}>💡 ذكر اليوم</Text>
            <View style={styles.zikrBox}>
              <Text style={styles.zikrText}>{azkarAr[currentZikrIndex]}</Text>
            </View>
            <TouchableOpacity 
              style={styles.secondaryButton} 
              onPress={() => setCurrentZikrIndex((currentZikrIndex + 1) % azkarAr.length)}
            >
              <Text style={styles.secondaryButtonText}>الذكر التالي</Text>
            </TouchableOpacity>
          </View>
        )}

        {currentTab === 'tasbeeh' && (
          <View style={styles.cardCenter}>
            <Text style={styles.cardTitle}>السبحة الإلكترونية المتطورة</Text>
            <View style={styles.counterCircle}>
              <Text style={styles.counterNumber}>{tasbeehCount}</Text>
              <Text style={styles.counterLabel}>تسبيحة</Text>
            </View>
            <View style={styles.row}>
              <TouchableOpacity style={styles.primaryButtonSquare} onPress={() => setTasbeehCount(tasbeehCount + 1)}>
                <Text style={styles.buttonText}>تسبيح</Text>
              </TouchableOpacity>
              <TouchableOpacity style={styles.dangerButtonSquare} onPress={() => setTasbeehCount(0)}>
                <Text style={styles.buttonText}>تصفير</Text>
              </TouchableOpacity>
            </View>
          </View>
        )}

        {currentTab === 'game' && (
          <View style={styles.card}>
            <Text style={styles.cardTitle}>لعبة المعادلات الرياضية وتنشيط الذاكرة</Text>
            <Text style={styles.scoreText}>النقاط الحالية: {gameScore} ⭐</Text>
            
            <View style={styles.questionBox}>
              <Text style={styles.questionText}>{question.num1} {question.op} {question.num2} = ؟</Text>
            </View>

            <TextInput 
              style={styles.inputCenter} 
              placeholder="أدخل الإجابة" 
              placeholderTextColor="#666"
              keyboardType="numeric"
              value={userAnswer}
              onChangeText={setUserAnswer}
            />

            <TouchableOpacity style={styles.primaryButton} onPress={checkAnswer}>
              <Text style={styles.buttonText}>تحقق من الإجابة</Text>
            </TouchableOpacity>
          </View>
        )}
      </ScrollView>

      {/* الفوتر السفلي (حقوق التطبيق الفخم) */}
      <View style={styles.footer}>
        <Text style={styles.footerText}>AinJamel Smart Platform © 2026</Text>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  // تنسيق شاشة الـ Splash الفخمة والمطابقة تماماً لصورتك
  splashContainer: {
    flex: 1,
    backgroundColor: '#121212',
    justifyContent: 'center',
    alignItems: 'center',
  },
  splashText: {
    fontSize: 42,
    color: '#F9F6EE',
    fontFamily: 'sans-serif-light',
    letterSpacing: 4,
    marginBottom: 30,
  },
  pnlTrack: {
    width: 300,
    height: 4,
    backgroundColor: '#282828',
    borderRadius: 2,
    overflow: 'hidden',
  },
  pnlProgress: {
    height: '100%',
    backgroundColor: '#F9F6EE',
  },

  // تنسيق منصة الواجهة الفخمة (كأنها موقع ويب)
  container: {
    flex: 1,
    backgroundColor: '#0d0d0d',
  },
  navbar: {
    height: 80,
    backgroundColor: '#161616',
    flexDirection: 'row-reverse',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: 20,
    borderBottomWidth: 1,
    borderBottomColor: '#262626',
    paddingTop: 20,
  },
  navLogo: {
    fontSize: 22,
    color: '#F9F6EE',
    fontWeight: 'bold',
    letterSpacing: 1,
  },
  navLinks: {
    flexDirection: 'row-reverse',
  },
  navBtn: {
    paddingVertical: 8,
    paddingHorizontal: 12,
    borderRadius: 6,
    marginLeft: 10,
  },
  activeNavBtn: {
    backgroundColor: '#F9F6EE',
  },
  navBtnText: {
    color: '#a6a6a6',
    fontSize: 14,
    fontWeight: '600',
  },
  activeNavBtnText: {
    color: '#121212',
  },
  contentWrapper: {
    padding: 20,
    alignItems: 'center',
  },
  card: {
    width: width > 500 ? 500 : '100%',
    backgroundColor: '#161616',
    borderRadius: 16,
    padding: 24,
    borderWidth: 1,
    borderColor: '#262626',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.3,
    shadowRadius: 5,
    elevation: 8,
  },
  cardCenter: {
    width: width > 500 ? 500 : '100%',
    backgroundColor: '#161616',
    borderRadius: 16,
    padding: 24,
    borderWidth: 1,
    borderColor: '#262626',
    alignItems: 'center',
  },
  cardTitle: {
    fontSize: 20,
    color: '#F9F6EE',
    fontWeight: 'bold',
    textAlign: 'center',
    marginBottom: 20,
  },
  cardSubTitle: {
    fontSize: 16,
    color: '#F9F6EE',
    fontWeight: '600',
    marginBottom: 12,
    textAlign: 'right',
  },
  input: {
    backgroundColor: '#222',
    color: '#fff',
    borderRadius: 8,
    padding: 12,
    textAlign: 'right',
    marginBottom: 16,
    fontSize: 15,
  },
  inputCenter: {
    backgroundColor: '#222',
    color: '#fff',
    borderRadius: 8,
    padding: 12,
    textAlign: 'center',
    marginBottom: 16,
    fontSize: 18,
  },
  primaryButton: {
    backgroundColor: '#F9F6EE',
    borderRadius: 8,
    paddingVertical: 14,
    alignItems: 'center',
    marginTop: 10,
  },
  buttonText: {
    color: '#121212',
    fontSize: 16,
    fontWeight: 'bold',
  },
  divider: {
    height: 1,
    backgroundColor: '#262626',
    marginVertical: 24,
  },
  zikrBox: {
    backgroundColor: '#1f1f1f',
    padding: 16,
    borderRadius: 8,
    borderRightWidth: 4,
    borderRightColor: '#F9F6EE',
    marginBottom: 12,
  },
  zikrText: {
    color: '#d4d4d4',
    fontSize: 15,
    lineHeight: 24,
    textAlign: 'right',
  },
  secondaryButton: {
    borderWidth: 1,
    borderColor: '#333',
    borderRadius: 8,
    paddingVertical: 10,
    alignItems: 'center',
  },
  secondaryButtonText: {
    color: '#a6a6a6',
    fontSize: 14,
  },
  counterCircle: {
    width: 160,
    height: 160,
    borderRadius: 80,
    backgroundColor: '#1f1f1f',
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 2,
    borderColor: '#262626',
    marginVertical: 20,
  },
  counterNumber: {
    fontSize: 48,
    color: '#F9F6EE',
    fontWeight: 'bold',
  },
  counterLabel: {
    fontSize: 14,
    color: '#666',
    marginTop: 4,
  },
  row: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    width: '100%',
    marginTop: 10,
  },
  primaryButtonSquare: {
    flex: 1,
    backgroundColor: '#F9F6EE',
    borderRadius: 8,
    paddingVertical: 14,
    alignItems: 'center',
    marginHorizontal: 5,
  },
  dangerButtonSquare: {
    flex: 1,
    backgroundColor: '#331a1a',
    borderWidth: 1,
    borderColor: '#552222',
    borderRadius: 8,
    paddingVertical: 14,
    alignItems: 'center',
    marginHorizontal: 5,
  },
  scoreText: {
    fontSize: 16,
    color: '#d4d4d4',
    textAlign: 'center',
    marginBottom: 15,
  },
  questionBox: {
    backgroundColor: '#1f1f1f',
    padding: 20,
    borderRadius: 12,
    alignItems: 'center',
    marginBottom: 20,
  },
  questionText: {
    fontSize: 32,
    color: '#F9F6EE',
    fontWeight: 'bold',
  },
  footer: {
    height: 50,
    justifyContent: 'center',
    alignItems: 'center',
    borderTopWidth: 1,
    borderTopColor: '#161616',
  },
  footerText: {
    color: '#444',
    fontSize: 12,
  },
});
